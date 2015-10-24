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

        private void button1_Click(object sender, EventArgs e)
        {
            buffer = new byte[2048];

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry ipHost = Dns.GetHostEntry("");
            IPAddress ipAddr = ipHost.AddressList[0];
            
            sock.Connect(ipAddr, 9000);
            if(!sock.Connected)
            {
                disconnectButton.Enabled = true;
            }

            sock.Send(buffer);
        }
    }
}
