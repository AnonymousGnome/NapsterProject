using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace ClientFormProject
{
    public partial class ClientForm : Form
    {
        private Socket sock, sockUDP, listenSock; // sockets for connecting to directory server
        private System.Windows.Forms.Timer timer; // timer for UDP hello message
        private int countDown = 10; // seconds timer waits until send next message
        private IPAddress hostIP; // holds ip for the central directory server
        private IPEndPoint ipEnd, listenSockEnd; // endpoints for hello message to directory server and listening socket
        private Dictionary<string, List<string>> peerFiles;

        byte[] buffer, helloMes; // bufferes for sending over network
        string path; // Filepath for shared files

        public ClientForm()
        {
            InitializeComponent();

            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(timerFunc);
            timer.Interval = 1000;

            helloMes = ASCIIEncoding.ASCII.GetBytes("Hello");
            messageLabel.Text = "";

            path = @".\SharedFiles";
            System.IO.Directory.CreateDirectory(path);
            peerFiles = new Dictionary<string, List<string>>();

            listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSockEnd = new IPEndPoint(IPAddress.Any, 9002);
            listenSock.Bind(listenSockEnd);
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            buffer = new byte[2048];

            //creates sockets for TCP and UDP connections
            sockUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                ConnectSocket(hostIP, 9000);
                //Takes input host ip address and establishes connection
                ipEnd = new IPEndPoint(hostIP, 9001);

                //activates buttons
                disconnectButton.Enabled = true;
                refreshButton.Enabled = true;
                registerButton.Enabled = false;

                
                /*
                 * This will send all file names in the SharedFiles folder to
                 * The directory server as well as retrieves available peer
                 * info from the directory server.
                 */
                string[] fileEntries = Directory.GetFiles(path); // retrieves filenames of all files in the SharedFiles folder
                string sendString = "";
                foreach (string s in fileEntries)
                {
                    string[] temp = s.Split('\\');
                    sendString +=  temp[temp.Length - 1] + ";";
                }

                buffer = ASCIIEncoding.ASCII.GetBytes(sendString);
                sock.Send(buffer);
                buffer = new byte[2048];
                sock.Receive(buffer);
                Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));

                string peers = ASCIIEncoding.ASCII.GetString(buffer);

                foreach(string s in peers.Split('?'))
                {
                    string[] files = s.Split(';');
                    List<string> temp = new List<string>();
                    for(int i = 1; i < files.Length; i++)
                    {
                        fileBox.Items.Add(files[i] + "\t\t\t" + files[0]);
                        temp.Add(files[i]);
                    }
                    peerFiles.Add(files[0], temp);
                }
                
                sock.Dispose();

                //starts hello message timer
                timer.Start();
                StartListening();
            }
            catch (SocketException socex)
            {
                messageLabel.Text = socex.Message;
                //messageLabel.Text = "Error connecting to server...";
            }
            catch(ArgumentNullException n)
            {
                messageLabel.Text = "No host has been specified...";
            }
            catch (Exception except)
            {
                messageLabel.Text = except.Message;
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            /*
             * This asks directory server to 
             * send fresh info about registered 
             * peers and available files for download
             */

            peerFiles.Clear();
            fileBox.Items.Clear();
            ConnectSocket(hostIP, 9000);

            string sendString = "REFRESH";
            buffer = ASCIIEncoding.ASCII.GetBytes(sendString);
            sock.Send(buffer);

            buffer = new byte[2048];
            sock.Receive(buffer);
            //Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));

            string peers = ASCIIEncoding.ASCII.GetString(buffer);

            foreach (string s in peers.Split('?'))
            {
                string[] files = s.Split(';');
                List<string> temp = new List<string>();
                for (int i = 1; i < files.Length; i++)
                {
                    fileBox.Items.Add(files[i] + "\t\t\t" + files[0]);
                    temp.Add(files[i]);
                }
                peerFiles.Add(files[0], temp);
            }

            sock.Dispose();
        }

        /*
         * Sends hello message via UDP socket to directory server and restarts timer
         */
        void timerFunc(object sender, EventArgs e)
        {
            countDown--;
            if (countDown < 1)
            {
                countDown = 10;

                sockUDP.SendTo(helloMes, ipEnd);
            }
        }

        /*
         * Stops hello message timer, clears file download list, and switches active and inactive buttons
         */
        private void disconnectButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
            countDown = 10;
            registerButton.Enabled = true;
            refreshButton.Enabled = false;
            disconnectButton.Enabled = false;
            messageLabel.Text = "Disconnected from server...";
            fileBox.Items.Clear();
            peerFiles.Clear();
        }

        /*
         * Opens up file directory for shared and downloaded files
         */
        private void openDirecButton_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(path))
                System.Diagnostics.Process.Start(path);
            else
                messageLabel.Text = "Cannot find folder SharedFiles...";
        }

        private void ConnectSocket(IPAddress ip, int port)
        {
            try
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                hostIP = IPAddress.Parse(hostIPText.Text);
                sock.Connect(hostIP, port);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        private void StartListening()
        {
            Thread listenThread = new Thread(ListenFunc);
            listenThread.Start();
        }

        void ListenFunc()
        {
            //continually loops for thread's lifetime
            while (true)
            {
                listenSock.Listen(10); //puts into listening state
                Socket newSock = listenSock.Accept(); //accepts incoming connection
                Console.WriteLine("Incoming Connection on port 9000...");
                Thread newCli = new Thread(new ParameterizedThreadStart(newCliFunc)); //generates new thread and socket for handling new client
                newCli.Start(newSock); //starts thread and passes socket to thread function
            }
        }

        void newCliFunc(object newSock)
        {
            byte[] buffer = new byte[2048];
            Socket tempSock = newSock as Socket;
            tempSock.Receive(buffer);
            Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {


            try
            {
                string ipKey = "";
                string requestedFile = fileBox.SelectedItem.ToString();
                foreach(var p in peerFiles)
                {
                    if(requestedFile.Contains(p.Key))
                    {
                        ipKey = p.Key;
                    }
                }
                ConnectSocket(IPAddress.Parse(ipKey), 9002);

                buffer = new byte[2048];

                buffer = ASCIIEncoding.ASCII.GetBytes(requestedFile);
                sock.Send(buffer);

                sock.Dispose();
            }
            catch(Exception ee)
            {

            }
        }
    }
}
