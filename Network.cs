using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SeaBattle.CSharp
{
    public class Network
    {
        private ShowMessageDelegate msgDelegate;
        private ReceiveShotDelegate shotDelegate;
        private ReceiveCellShotResultDelegate shotResultDelegate;
        private UdpClient udpListener;
        private UdpClient udpSender;
        private Thread _thrListen;
        private bool _terminated;

        private const string cmdSendShot = "sht";
        private const string cmdShotResult = "shr";
        private const string cmdChatMsg = "msg";
        public Network (ShowMessageDelegate MsgDelegate, 
                        ReceiveShotDelegate ShotDelegate, 
                        ReceiveCellShotResultDelegate ShotResultDelegate)
        {
            msgDelegate = MsgDelegate;
            shotDelegate = ShotDelegate;
            shotResultDelegate = ShotResultDelegate;
            _terminated = false;
        }
        public void Connect(string MyIP, string FriendIP, int MyPort, int FriendPort)
        {
            udpListener = new UdpClient(MyPort);
            udpSender = new UdpClient();
            udpSender.Connect(FriendIP, FriendPort);
            _thrListen = new Thread(new ThreadStart(ListenUdp));
            _thrListen.Start();
        }
        //отправка сообщения в чат
        public void SendMessage(string Message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(cmdChatMsg + " " + Message);
            udpSender.Send(bytes, bytes.Length);
        }
        //выстрел в поле соперника
        public void SendShot(int X, int Y)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(cmdSendShot + " " + X.ToString() + " " + Y.ToString());
            udpSender.Send(bytes, bytes.Length); 
        }
        //отправка результата выстрела соперника
        public void SendCellShotResult(int X, int Y, ShotResult State)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(cmdShotResult + " " + 
                X.ToString() + " " + Y.ToString() + " " + ((int)State).ToString());
            udpSender.Send(bytes, bytes.Length);
        }
        //прием результата выстрела в поле соперника
        public void ReceiveCellShotResult(string ResultString)
        {
            string[] resultsSplitted = ResultString.Split(' ');
            shotResultDelegate.Invoke(Convert.ToInt32(resultsSplitted[0]), Convert.ToInt32(resultsSplitted[1]), 
                (ShotResult)Convert.ToInt32(resultsSplitted[2]));
        }
        //прием выстрела от соперника
        public void ReceiveShot(string Coords)
        {
            string[] coordsSplitted = Coords.Split(' ');
            shotDelegate.Invoke(Convert.ToInt32(coordsSplitted[0]), Convert.ToInt32(coordsSplitted[1]));
        }

        //разрыв соединения
        public void Disconnect()
        {
            if (udpListener != null)
            {
                udpListener.Close();
                _terminated = true;
                _thrListen.Interrupt();
                _thrListen.Join();
            }
        }
        //Поток приёма данных по UDP
        private void ListenUdp()
        {
            IPEndPoint RemoteIpEndPoint = null;

            byte[] receiveBytes;
            while (!_terminated)
            {
                // Ожидание датаграммы
                try
                {
                    receiveBytes = udpListener.Receive(ref RemoteIpEndPoint);
                }
                catch(Exception)
                {
                    continue;
                }

                string returnData = Encoding.UTF8.GetString(receiveBytes);
                string cmd = returnData.Substring(0, 3);
                string msg = returnData.Substring(4);
                returnData = returnData.Remove(0, 1);   //убираем идентификатор команды
                if (cmd == cmdChatMsg)
                {
                    // Преобразуем и отображаем данные
                    returnData = returnData.Remove(0, 1);   //убираем идентификатор команды
                    msgDelegate.Invoke(msg);
                }
                else if (cmd == cmdSendShot)
                {
                    ReceiveShot(msg);
                }
                else if (cmd == cmdShotResult)
                {
                    ReceiveCellShotResult(msg);
                }
            }
        }
    }
}
