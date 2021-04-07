using System;
using System.Threading;
using TestConsolAppDotNetCore.Utils;

namespace TestConsolAppDotNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            int port;

            try
            {
                if (args.Length == 0)
                    throw new Exception("Socket is not detrmined!");

                if (!int.TryParse(args[0], out port))
                    throw new Exception("Socket is bad!");

                var ipAddr = IpAddressUtilities.GetLocalIPAddress();

                Thread server = new Thread(delegate ()
                {
                    SocketServer myserver = new SocketServer(ipAddr, port);
                });
                server.Start();

                Console.WriteLine("Server Started!");

                while (true)
                {
                    string s = Console.ReadLine();
                    if (s.Trim().ToLower() == "exit")
                    {
                        Environment.Exit(-1);
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }
    }
}
