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

namespace ClientFormProject
{
    public partial class ClientForm : Form
    {
        private Socket sock, sockUDP;
        private System.Windows.Forms.Timer timer;
        private int countDown = 10;
        private IPAddress hostIP;
        private IPEndPoint ipEnd;
        byte[] buffer, helloMes;

        public ClientForm()
        {
            InitializeComponent();
            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(timerFunc);
            timer.Interval = 1000;
            helloMes = ASCIIEncoding.ASCII.GetBytes("Hello");
            messageLabel.Text = "";
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            buffer = new byte[2048];

            //creates sockets for TCP and UDP connections
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sockUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                //Takes input host ip address and establishes connection
                hostIP = IPAddress.Parse(hostIPText.Text);
                sock.Connect(hostIP, 9000);
                ipEnd = new IPEndPoint(hostIP, 9001);

                //activates buttons
                disconnectButton.Enabled = true;
                refreshButton.Enabled = true;
                registerButton.Enabled = false;

                //sock.Send(buffer);

                //starts hello message timer
                timer.Start();
            }
            catch (SocketException socex)
            {
                messageLabel.Text = socex.Message;
                //errorLabel.Text = "Error connecting to server...";
            }
            catch (Exception except)
            {
                messageLabel.Text = except.Message;
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {

        }

        void timerFunc(object sender, EventArgs e)
        {
            countDown--;
            if (countDown < 1)
            {
                countDown = 10;

                sockUDP.SendTo(helloMes, ipEnd);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
            countDown = 10;
            registerButton.Enabled = true;
            refreshButton.Enabled = false;
            disconnectButton.Enabled = false;
            messageLabel.Text = "Disconnected from server...";
            fileBox.Items.Clear();
        }
    }
}
