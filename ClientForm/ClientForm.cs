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
        private Socket sock, sockUDP; // sockets for connecting to directory server
        private System.Windows.Forms.Timer timer; // timer for UDP hello message
        private int countDown = 10; // seconds timer waits until send next message
        private IPAddress hostIP; // holds ip for the central directory server
        private IPEndPoint ipEnd; // endpoint for hello message to directory server
        private Dictionary<string, string> peerFiles;

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
            peerFiles = new Dictionary<string, string>();
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            buffer = new byte[2048];

            //creates sockets for TCP and UDP connections
            sockUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                ConnectSocket(hostIP);
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
                sock.Receive(buffer);
                Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));

                string peers = ASCIIEncoding.ASCII.GetString(buffer);

                foreach(string s in peers.Split('?'))
                {
                    string[] files = s.Split(';');
                    for(int i = 1; i < files.Length; i++)
                    {
                        fileBox.Items.Add(files[i] + "\t\t\t" + files[0]);
                        peerFiles.Add(files[i], files[0]);
                    }
                }
                
                sock.Dispose();

                //starts hello message timer
                timer.Start();
            }
            catch (SocketException socex)
            {
                //messageLabel.Text = socex.Message;
                messageLabel.Text = "Error connecting to server...";
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
            ConnectSocket(hostIP);

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
                for (int i = 1; i < files.Length; i++)
                {
                    fileBox.Items.Add(files[i] + "\t\t\t" + files[0]);
                    peerFiles.Add(files[i], files[0]);
                }
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

        private void ConnectSocket(IPAddress ip)
        {
            try
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                hostIP = IPAddress.Parse(hostIPText.Text);
                sock.Connect(hostIP, 9000);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
