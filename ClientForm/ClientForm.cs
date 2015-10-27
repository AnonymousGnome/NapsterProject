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
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace ClientFormProject
{
    public partial class ClientForm : Form
    {
        private Socket sock, sockUDP, listenSock; // sockets for connecting to directory server
        private System.Windows.Forms.Timer timer; // timer for UDP hello message
        private int countDown = 60; // seconds timer waits until send next message
        private IPAddress hostIP; // holds ip for the central directory server
        private IPEndPoint ipEnd, listenSockEnd; // endpoints for hello message to directory server and listening socket
        private int listenPort;

        byte[] buffer, helloMes; // bufferes for sending over network
        string path; // Filepath for shared files

        public ClientForm()
        {
            InitializeComponent();

            //timer for UDP hello message
            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(timerFunc);
            timer.Interval = 1000;

            helloMes = ASCIIEncoding.ASCII.GetBytes("Hello");
            messageLabel.Text = "";

            //Creates required directory
            path = @".\SharedFiles";
            System.IO.Directory.CreateDirectory(path);

            
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            buffer = new byte[2048];

            //creates sockets for TCP and UDP connections
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sockUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            

            try
            {
                //ConnectSocket(hostIP, 9000);
                //Takes input host ip address and establishes connection
                hostIP = IPAddress.Parse(hostIPText.Text);
                listenPort = Int32.Parse(portText.Text);
                ipEnd = new IPEndPoint(hostIP, 9001);
                listenSockEnd = new IPEndPoint(IPAddress.Any, listenPort);
                listenSock.Bind(listenSockEnd);
                sock.Connect(hostIP, 9000);

                //activates buttons
                disconnectButton.Enabled = true;
                registerButton.Enabled = false;

                
                /*
                 * This will send all file names in the SharedFiles folder to
                 * The directory server as well as retrieves available peer
                 * info from the directory server.
                 */
                string[] fileEntries = Directory.GetFiles(path); // retrieves filenames of all files in the SharedFiles folder
                string sendString = listenPort + ";";
                foreach (string s in fileEntries)
                {
                    string[] temp = s.Split('\\');
                    sendString +=  temp[temp.Length - 1] + ";";
                }

                /*
                 * Populates list box with recieved peer info
                 */
                buffer = ASCIIEncoding.ASCII.GetBytes(sendString);
                sock.Send(buffer);
                buffer = new byte[2048];
                sock.Receive(buffer);
                Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));
                string peers = ASCIIEncoding.ASCII.GetString(buffer);
                foreach (string s in peers.Split('?'))
                {
                    string[] files = s.Split(';');
                    List<string> temp = new List<string>();
                    for (int i = 1; i < files.Length; i++)
                    {
                        fileBox.Items.Add(files[i] + "\t\t\t" + files[0]);
                    }
                }

                //closes connection
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();

                //starts hello message timer
                timer.Start();
                StartListening();
            }
            catch (SocketException socex)
            {
                messageLabel.Text = socex.Message;
                messageLabel.Text = "Error connecting to server...";
                Console.WriteLine(socex.Message);
            }
            catch (ArgumentNullException n)
            {
                messageLabel.Text = "No host has been specified...";
                Console.WriteLine(n.Message);
            }
            catch (Exception except)
            {
                messageLabel.Text = except.Message;
                Console.WriteLine(except.Message);
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            /*
             * This asks directory server to 
             * send fresh info about registered 
             * peers and available files for download
             */
            fileBox.Items.Clear();

            string sendString = "REFRESH";
            buffer = ASCIIEncoding.ASCII.GetBytes(sendString);
            //sock.Send(buffer);

            buffer = new byte[2048];
            //sock.Receive(buffer);
            //Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));

            string peers = ASCIIEncoding.ASCII.GetString(buffer);

            foreach (string s in peers.Split('?'))
            {
                string[] files = s.Split(';');
                List<string> temp = new List<string>();
                for (int i = 1; i < files.Length; i++)
                {
                    fileBox.Items.Add(files[i] + "\t\t\t" + files[0]);
                }
            }

            //sock.Shutdown(SocketShutdown.Both);
            //sock.Close();
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
            try
            {
                timer.Stop();
                countDown = 10;
                registerButton.Enabled = true;
                refreshButton.Enabled = false;
                disconnectButton.Enabled = false;
                fileBox.Items.Clear();

                sock.Shutdown(SocketShutdown.Both);
                sock.Close();

                messageLabel.Text = "Disconnected from server...";
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
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

        private void StartListening()
        {
            Thread listenThread = new Thread(ListenFunc);
            listenThread.Start();
        }

        void ListenFunc()
        {
            try
            {
                //continually loops for thread's lifetime
                while (true)
                {
                    listenSock.Listen(10); //puts into listening state
                    Socket newSock = listenSock.Accept(); //accepts incoming connection
                    messageLabel.Text = "Incoming Connection on port " + listenPort;
                    Thread newCli = new Thread(new ParameterizedThreadStart(newCliFunc)); //generates new thread and socket for handling new client
                    newCli.Start(newSock); //starts thread and passes socket to thread function
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /*
         * Function for handling new clients
         */
        void newCliFunc(object newSock)
        {
            try
            {
                string dir = Directory.GetCurrentDirectory() + @"\SharedFiles"; // full directory for shared files folder
                Socket tempSock = newSock as Socket; // temporary socket reference
                tempSock.Receive(buffer); // receieves size of incoming data
                buffer = new byte[Int32.Parse(ASCIIEncoding.ASCII.GetString(buffer))]; // creates buffer of size
                tempSock.Receive(buffer); // recieves incoming data
                string filePath = dir + @"\" + ASCIIEncoding.ASCII.GetString(buffer).Trim(); // appends filename to directory
                byte[] bytes = System.IO.File.ReadAllBytes(@filePath); // Reads all bytes from file
                string size = bytes.Length.ToString(); // size of outgoing file
                tempSock.Send(ASCIIEncoding.ASCII.GetBytes(size)); // sends size to client
                tempSock.Send(bytes); // sends file to client
            }
            catch(Exception e)
            {
                messageLabel.Text = e.Message;
            }
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            Socket peerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                string[] requestedFileArray = fileBox.SelectedItem.ToString().Split('\t'); // takes requested file info from listbox

                peerSock.Connect(IPAddress.Parse(requestedFileArray[3].Split('_')[0]), Int32.Parse(requestedFileArray[3].Split('_')[1])); // connects to peer

                buffer = new byte[2048];

                if (peerSock.Connected)
                {
                    messageLabel.Text = "Client connected. Starting to receive the file";

                    byte[] bytes = ASCIIEncoding.ASCII.GetBytes(requestedFileArray[0]); // creates byte array of requested file
                    string size = bytes.Length.ToString(); // size of request
                    peerSock.Send(ASCIIEncoding.ASCII.GetBytes(size)); //sends size of request
                    peerSock.Send(bytes); //sends request

                    peerSock.Receive(buffer); // receieves size of file
                    buffer = new byte[Int32.Parse(ASCIIEncoding.ASCII.GetString(buffer))]; // creates buffer size of file
                    peerSock.Receive(buffer); // receieves file
                    File.WriteAllBytes(path + "\\" + requestedFileArray[0], buffer); // recontructs file locally

                    //closes socket
                    peerSock.Shutdown(SocketShutdown.Both);
                    peerSock.Close();
                }
                else
                    messageLabel.Text = "Cannot connect to client";
            }
            catch (Exception ee)
            {
                messageLabel.Text = ee.Message;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

}
