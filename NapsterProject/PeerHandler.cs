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

        public PeerHandler(string path)
        {
            this.path = path;
        }

        public void ReceivePeer(IPEndPoint ipEnd)
        {

        }
    }
}
