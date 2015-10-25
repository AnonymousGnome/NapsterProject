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
        private Timer timer; // timer for UDP hello message
        private int countDown = 1; // seconds timer waits until send next message
        private Dictionary<IPAddress, int> peerTimers;
        private int timeToWait;

        public PeerHandler(string path)
        {
            this.path = path;
            TimerCallback callback = new TimerCallback(CleanList);
            timer = new Timer(callback, null, 0, 1000);
            peerTimers = new Dictionary<IPAddress, int>();
            timeToWait = 30;
        }

        public void ReceivePeer(IPEndPoint ipEnd)
        {
            peerTimers.Add(ipEnd.Address, timeToWait);
        }

        public void UpdateClient(EndPoint sender)
        {
            EndPoint ipEnd = sender;

            foreach(KeyValuePair<IPAddress, int> p in peerTimers)
            {
                if(p.Key == IPAddress.Parse(ipEnd.ToString().Split(':')[0]))
                {
                    peerTimers[p.Key] = timeToWait;
                }
            }
        }

        void CleanList(object o)
        {
            countDown--;
            if (countDown < 1)
            {
                countDown = 1;

                foreach(var p in peerTimers)
                {
                    int value = p.Value;
                    value--;
                    Console.WriteLine(p.Value);
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
