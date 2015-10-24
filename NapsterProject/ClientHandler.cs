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
        private int portNoTCP, portNoUDP;
        private Socket socketTCP, socketUDP;
        private IPEndPoint ipEndPointTCP, ipEndPointUDP;

        public ClientHandler()
        {
            //Stores port numbers
            this.portNoTCP = 9000;
            portNoUDP = 9001;

            //Creates sockets for TCP and UDP listeners
            

            //Resolves local IP and associates ports
            //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddr = ipHost.AddressList[0];
            //IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];

            socketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipEndPointTCP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), portNoTCP);
            ipEndPointUDP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), portNoUDP);

            //Binds sockets to ports
            socketTCP.Bind(ipEndPointTCP);
            socketUDP.Bind(ipEndPointUDP);
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
                Console.WriteLine("Opening TCP listener...");
                socketTCP.Listen(10); //puts into listening state
                Socket newSock = socketTCP.Accept(); //accepts incoming connection
                Console.WriteLine("Incoming Connection on port 9000...");
                Thread newCli = new Thread(new ParameterizedThreadStart(newCliFunc)); //generates new thread and socket for handling new client
                newCli.Start(newSock); //starts thread and passes socket to thread function
            }
        }

        private void udpFunc()
        {

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
