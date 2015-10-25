using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace NapsterProject
{
    class PeerHandler
    {
        private string path;
        private Timer timer; // timer for UDP hello message
        private int countDown = 1; // seconds timer waits until send next message
        private ConcurrentDictionary<IPAddress, int> peerTimers;
        private int timeToWait;

        public PeerHandler(string path)
        {
            this.path = path;
            TimerCallback callback = new TimerCallback(CleanList);
            timer = new Timer(callback, null, 0, 1000);
            peerTimers = new ConcurrentDictionary<IPAddress, int>();
            timeToWait = 30;
        }

        public void ReceivePeer(EndPoint ipEnd)
        {
            peerTimers.TryAdd(IPAddress.Parse(ipEnd.ToString().Split(':')[0]), timeToWait);
        }

        public void UpdateClient(EndPoint sender)
        {
            EndPoint ipEnd = sender;

            foreach(KeyValuePair<IPAddress, int> p in peerTimers)
            {
                peerTimers.AddOrUpdate(IPAddress.Parse(ipEnd.ToString().Split(':')[0]), timeToWait, (k, v) => timeToWait);
            }
        }

        void CleanList(object o)
        {
            Console.WriteLine("Cleaning list...");
            countDown--;
            if (countDown < 1)
            {
                countDown = 1;
                try
                {
                    foreach (KeyValuePair<IPAddress, int> p in peerTimers)
                    {
                        int value = p.Value;
                        value--;
                        Console.WriteLine("New value: {0}", value);
                        Console.WriteLine("Old Value: {0}", p.Value);
                        Console.WriteLine(p.Value);
                        if (value < 1)
                        {
                            Console.WriteLine("Removed {0} from list.", p.Key);
                            int i;
                            peerTimers.TryRemove(p.Key, out i);
                        }
                        else
                        {
                            peerTimers[p.Key] = value;
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }
        }
    }
}
