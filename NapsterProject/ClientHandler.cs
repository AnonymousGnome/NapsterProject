﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace NapsterProject
{
    class ClientHandler
    {
        private TcpListener socketTCP;
        private Socket socketUDP;
        private IPEndPoint ipEndPointTCP, ipEndPointUDP;
        private string path;
        private PeerHandler peerHandler;

        public ClientHandler()
        {
            //Creates sockets for TCP and UDP listeners
            socketTCP = new TcpListener(IPAddress.Any, 9000);
            socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipEndPointUDP = new IPEndPoint(IPAddress.Any, 9001);

            //Binds sockets to ports
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
                socketTCP.Start(10); //puts into listening state
                Socket newSock = socketTCP.AcceptSocket(); //accepts incoming connection
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
                peerHandler.UpdateClient(Remote); // passes peer info to handler to restart timer
                //Console.WriteLine("Message received from {0}:", Remote.ToString());
                //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
            }
        }

        /*
         * This will place client's available file info into 
         * the directory file in the PeerDirectory folder
         * and send the client the available info on registered
         * peers (peer ips and files available to download)
         */
        private void newCliFunc(object newSock)
        {
            Socket tempSock = newSock as Socket; //retrieves socket from passed parameter
            byte[] buffer = new byte[2048]; //buffer for receiving info from client
            peerHandler.ReceivePeer(tempSock.RemoteEndPoint);
            int size = tempSock.Receive(buffer); //receives info from client
            if (!ASCIIEncoding.ASCII.GetString(buffer).Contains("REFRESH"))
            {
                char[] delimiters = { ';' };
                addToFileList(new List<string>(Encoding.ASCII.GetString(buffer, 0, size).Split(delimiters)), tempSock.RemoteEndPoint);
            }

            string message = compilePeers(tempSock);
            //Console.WriteLine(message);
            buffer = ASCIIEncoding.ASCII.GetBytes(message);
            tempSock.Send(buffer);

            
            //message = ASCIIEncoding.ASCII.GetString(buffer);
            //Console.WriteLine(message.Substring(0, message.LastIndexOf(';')));
        }

        private void addToFileList(List<string> files, EndPoint endPoint)
        {
            string directoryFile = @".\PeerDirectory\" + endPoint.ToString().Split(':')[0] + "_" + files.ElementAt(0) + ".txt"; // The file where we want to save the client/file informaiton
            files.RemoveAt(0); // removes port number from beginning of file list
            File.WriteAllLines(directoryFile, files); // Write all the data in the list to the file.
            // Save the list of files from the client to the directory.
        }

        /*
         * Compiles all available peers and their shared files to send to client
         * 
         * note this no longer is able to sort out clients own files form list
         * ran out of time to fix
         */
        private string compilePeers(Socket tempSock)
        {
            DirectoryInfo peers = new DirectoryInfo(path); // Directory of peer registry
            string ipString = tempSock.RemoteEndPoint.ToString().Split(':')[0];
            string message = "";
            foreach (FileInfo file in peers.GetFiles())
            {
                if (file.Name != ipString + ".txt") // supposed to ignore file corresponding to requesting peer
                {
                    message += file.Name.Substring(0,file.Name.LastIndexOf('.')) + ";";
                    StreamReader sr = new StreamReader(path + "\\" + file.Name);
                    while (sr.Peek() != -1)
                    {
                        message += sr.ReadLine() + ";";
                    }
                    message += "?";

                    sr.Close();
                }
            }
            if (message.Length == 0)
            {
                return "No Available Peers";
            }
            else
            {
                return message;
            }
        }
    }
}
