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
        private Socket sock;
        byte[] buffer;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            buffer = new byte[2048];

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                string ipString = hostIPText.Text;
                hostIPText.Text = "";
                sock.Connect(IPAddress.Parse(ipString), 9000);

                disconnectButton.Enabled = true;
                refreshButton.Enabled = true;

                //sock.Send(buffer);
            }
            catch (SocketException socex)
            {
                errorLabel.Text = socex.Message;
                //errorLabel.Text = "Error connecting to server...";
            }
            catch (Exception except)
            {
                errorLabel.Text = except.Message;
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {

        }
    }
}
