using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Blockchain blockchain = new Blockchain();
            string host, port;
            Console.WriteLine("Input host and port");
            host = Console.ReadLine();
            port = Console.ReadLine();
            Server server = new Server(blockchain,host,port);
            Console.ReadLine();
        }
    }
}
