using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapsterProject
{
    class Program
    {

        static void Main(string[] args)
        {
            ClientHandler handler = new ClientHandler();

            handler.StartListening();
        }
    }
}
