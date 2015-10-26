using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

namespace NapsterProject
{
    class PeerHandler
    {
        private string path; // filepath for the peer directory
        private Timer timer; // timer for UDP hello message
        private int countDown = 1; // seconds timer waits until send next message
        private ConcurrentDictionary<IPAddress, int> peerTimers; // thread safe dictionary that holds peer ips and time since last udp hello
        private int timeToWait; // time to allow between udp hello messages before being removed from the registry

        public PeerHandler(string path)
        {
            this.path = path;

            //creates timer
            TimerCallback callback = new TimerCallback(CleanList);
            timer = new Timer(callback, null, 0, 1000);
            peerTimers = new ConcurrentDictionary<IPAddress, int>();
            timeToWait = 30;
        }

        /*
         * Takes in new peer and adds them to the timer dictionary
         */
        public void ReceivePeer(EndPoint ipEnd)
        {
            peerTimers.TryAdd(IPAddress.Parse(ipEnd.ToString().Split(':')[0]), timeToWait);
        }

        /*
         * Restarts the peer's timer in the timer dicitonary
         */
        public void UpdateClient(EndPoint sender)
        {
            EndPoint ipEnd = sender;

            foreach(KeyValuePair<IPAddress, int> p in peerTimers)
            {
                peerTimers.AddOrUpdate(IPAddress.Parse(ipEnd.ToString().Split(':')[0]), timeToWait, (k, v) => timeToWait);
            }
        }

        /*
         * Timer function, each second it will go through the dictionary,
         * decrementing each timer value down by a second and removing expired peers
         */
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
                            DirectoryInfo peers = new DirectoryInfo(path);

                            foreach (FileInfo file in peers.GetFiles())
                            {
                                if(file.Name == p.Key.ToString() + ".txt")
                                    file.Delete();
                            }
                            //foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                            //{
                            //    dir.Delete(true);
                            //}
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
