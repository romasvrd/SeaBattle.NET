using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SeatBattle.CSharp
{
    class Network
    {
        private ShowMessageDelegate msgDelegate;
        private UdpClient udpListener;
        private UdpClient udpSender;
        private Thread _thrListen;
        private bool _terminated;
        public Network(ShowMessageDelegate MsgDelegate)
        {
            msgDelegate = MsgDelegate;
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

        public void Send(string Message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Message);
            udpSender.Send(bytes, bytes.Length);
        }
        private void ListenUdp()
        {
            IPEndPoint RemoteIpEndPoint = null;
            while (!_terminated)
            {
                // Ожидание дейтаграммы
                byte[] receiveBytes = udpListener.Receive(ref RemoteIpEndPoint);

                // Преобразуем и отображаем данные
                string returnData = Encoding.UTF8.GetString(receiveBytes);
                msgDelegate.Invoke(returnData);
                //Console.WriteLine(" --> " + returnData.ToString());
            }
        }
    }
}
