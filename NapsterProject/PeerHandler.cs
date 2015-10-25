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
        private Dictionary<IPAddress, int> peerTimers;

        public PeerHandler(string path)
        {
            this.path = path;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(CleanList);
            timer.Interval = 1000;
            timer.Start();
        }

        public void ReceivePeer(IPEndPoint ipEnd)
        {
            peerTimers.Add(ipEnd.Address, 200);
        }

        public void UpdateClient(EndPoint sender)
        {
            EndPoint ipEnd = sender;

            foreach(var p in peerTimers)
            {
                if(p.Key == IPAddress.Parse(ipEnd.ToString().Split(':')[0]))
                {
                    peerTimers[p.Key] = 200;
                }
            }
        }

        private void CleanList(object sender, EventArgs e)
        {
            countDown--;
            if (countDown < 1)
            {
                countDown = 1;

                foreach(var p in peerTimers)
                {
                    int value = p.Value;
                    value--;
                    if(value < 1)
                    {
                        Console.WriteLine("Removed {0} from list.", p.Key);
                        peerTimers.Remove(p.Key);
                    }
                    else
                    {
                        peerTimers[p.Key] = value;
                    }
                }
            }
        }
    }
}
