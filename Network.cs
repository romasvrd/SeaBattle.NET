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
        private UdpClient udpListener;
        private UdpClient udpSender;
        private Thread _thrListen;
        private GameController controller;
        private bool _terminated;

        private const string cmdSendShot = "sht";
        private const string cmdShotResult = "shr";
        private const string cmdChatMsg = "msg";
        public Network(ShowMessageDelegate MsgDelegate, ReceiveShotDelegate ShotDelegate, GameController Controller)
        {
            controller = Controller;
            msgDelegate = MsgDelegate;
            shotDelegate = ShotDelegate;
            _terminated = false;
        }
        ~Network()
        {
            _terminated = true;
        }
        public void Connect(string MyIP, string FriendIP, int MyPort, int FriendPort)
        {
            udpListener = new UdpClient(MyPort);
            udpSender = new UdpClient();
            udpSender.Connect(FriendIP, FriendPort);
            _thrListen = new Thread(new ThreadStart(ListenUdp));
            _thrListen.Start();
        }

        public void Send(string Message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(cmdChatMsg + " " + Message);
            udpSender.Send(bytes, bytes.Length);
        }

        public void SendShot(int X, int Y)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(cmdSendShot + " " + X.ToString() + " " + Y.ToString());
            udpSender.Send(bytes, bytes.Length); 
        }
        public void ReceiveShot(string Coords)
        {
            string[] coordsSplitted = Coords.Split(' ');

            shotDelegate.Invoke(Convert.ToInt32(coordsSplitted[0]), Convert.ToInt32(coordsSplitted[1]));
            //controller.shootResult(Convert.ToInt32(coordsSplitted[0]), Convert.ToInt32(coordsSplitted[1]));
        }
        private void ListenUdp()
        {
            IPEndPoint RemoteIpEndPoint = null;
            while (!_terminated)
            {
                // Ожидание датаграммы
                byte[] receiveBytes = udpListener.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.UTF8.GetString(receiveBytes);
                string cmd = returnData.Substring(0, 3);
                string msg = returnData.Substring(4);
                returnData = returnData.Remove(0, 1);   //убираем идентификатор команды
                msgDelegate.Invoke("input" + msg);
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

                }
            }
        }
    }
}
