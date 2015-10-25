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
    class PeerHandler
    {
        private string path;
        private System.Windows.Forms.Timer timer; // timer for UDP hello message
        private int countDown = 1; // seconds timer waits until send next message

        public PeerHandler(string path)
        {
            this.path = path;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(CleanList);
            timer.Interval = 1000;
        }

        public void ReceivePeer(IPEndPoint ipEnd)
        {

        }

        public void UpdateClient(EndPoint sender)
        {
            EndPoint ipEnd = sender;

            Console.WriteLine(ipEnd + ":peerIP");
        }

        private void CleanList(object sender, EventArgs e)
        {
            countDown--;
            if (countDown < 1)
            {
                countDown = 1;


            }
        }
    }
}
