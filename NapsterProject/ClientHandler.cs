using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NapsterProject
{
    class ClientHandler
    {
        private Socket socketTCP, socketUDP;
        private IPEndPoint ipEndPointTCP, ipEndPointUDP;
        private string path;
        private PeerHandler peerHandler;

        public ClientHandler()
        {
            //Stores port numbers

            //Creates sockets for TCP and UDP listeners
            socketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipEndPointTCP = new IPEndPoint(IPAddress.Any, 9000);
            ipEndPointUDP = new IPEndPoint(IPAddress.Any, 9001);

            //Binds sockets to ports
            socketTCP.Bind(ipEndPointTCP);
            socketUDP.Bind(ipEndPointUDP);

            //Creates directory for storing registered peers
            path = @".\PeerDirectory";
            System.IO.Directory.CreateDirectory(path);

            //Creates handler for managing registered peers
            peerHandler = new PeerHandler(path);
        }

        public void StartListening()
        {
            //Generates threads for each port listener
            Thread tcpThread = new Thread(tcpFunc);
            Thread udpThread = new Thread(udpFunc);
            tcpThread.Start();
            udpThread.Start();
        }

        private void tcpFunc()
        {
            //continually loops for thread's lifetime
            while (true)
            {
                socketTCP.Listen(10); //puts into listening state
                Socket newSock = socketTCP.Accept(); //accepts incoming connection
                Console.WriteLine("Incoming Connection on port 9000...");
                Thread newCli = new Thread(new ParameterizedThreadStart(newCliFunc)); //generates new thread and socket for handling new client
                newCli.Start(newSock); //starts thread and passes socket to thread function
            }
        }

        private void udpFunc()
        {
            byte[] data = new byte[2048];
            
            //continually loops for thread's lifetime
            while (true)
            {
                Console.WriteLine("Waiting for client");
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = socketUDP.ReceiveFrom(data, ref Remote);
                Console.WriteLine("Message received from {0}:", Remote.ToString());
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
            }
        }

        private void newCliFunc(object newSock)
        {
            Socket tempSock = newSock as Socket; //retrieves socket from passed parameter
            byte[] buffer = new byte[2048]; //buffer for receiving info from client
            tempSock.Receive(buffer); //receives info from client
            Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));
        }
    }
}
