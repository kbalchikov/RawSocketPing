using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine.Utility;
using System.Net;
using System.Diagnostics;

namespace MyPing
{
    class Program
    {
        static int Main(string[] args)
        {
            Arguments commandLine = new Arguments(args);
            string host = args.Last<string>();
            int count = 4;
            int timeout = 1000;
            short ttl = 128;
            bool infinite = false;
            ushort buffersize = 32;
            bool enableFragment = false;

            if (args == null || commandLine["?"] != null)
            {
                PrintHelp();
                return 0;
            }

            if (commandLine["n"] != null)
                if (!int.TryParse(commandLine["n"], out count))
                {
                    PrintError("Invalid argument for option -n");
                    return 1;
                }

            if (commandLine["t"] != null)
                infinite = true;

            if (commandLine["l"] != null)
            {
                if (!ushort.TryParse(commandLine["l"], out buffersize))
                {
                    PrintError("Invalid argument for option -l");
                    return 1;
                }
            }

            if (commandLine["w"] != null)
            {
                if (!int.TryParse(commandLine["w"], out timeout))
                {
                    PrintError("Invalid argument for option -w");
                    return 1;
                }
            }

            if (commandLine["i"] != null)
            {
                if (!short.TryParse(commandLine["i"], out ttl))
                {
                    PrintError("Invalid argument for option -i");
                    return 1;
                }
            }

            if (commandLine["f"] != null)
            {
                enableFragment = true;
            }

            // Trying to resolve the specified IP address
            IPAddress hostIP;
            try
            {
                hostIP = Dns.GetHostAddresses(host)[0];
            }
            catch
            {
                Console.WriteLine("Unable to resolve the specified hostname.");
                return 1;
            }

            //Start pinging
            try
            {
                if (host.Equals(hostIP.ToString()))
                    Console.WriteLine("\r\nPinging {0} with {1} bytes of data:\r\n",
                        host, buffersize.ToString());
                else Console.WriteLine("\r\nPinging {0} [{1}] with {2} bytes of data:\r\n",
                    host, hostIP.ToString(), buffersize.ToString());

                using (IcmpClient client = new IcmpClient(hostIP))
                {
                    TimeSpan time;
                    while (infinite || count > 0)
                    {
                        try
                        {
                            time = client.Ping(timeout, buffersize, !enableFragment, ttl);
                            if (time.Equals(TimeSpan.MaxValue)) Console.WriteLine("Request timed out.");
                            else Console.WriteLine("Reply from {0}: bytes={1} time={2} ms",
                                hostIP, buffersize, Math.Round(time.TotalMilliseconds));

                            if (time.TotalMilliseconds < 1000)
                            {
                                System.Threading.Thread.Sleep(1000 - (int)time.TotalMilliseconds);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Network error: " + e.Message);
                            Debug.WriteLine(e.ToString());
                        }
                        if (!infinite) count--;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error while pinging the specified host.");
            }

            Console.WriteLine("Test: -n: {0}, -t: {1}, -l: {2}, -w: {3}, -i: {4}, -f: {5}",
                count.ToString(), infinite.ToString(), buffersize.ToString(), timeout.ToString(),
                ttl.ToString(), enableFragment.ToString());

            Console.WriteLine("Hostname: " + args.Last<string>());

            Console.ReadLine();

            return 0;
        }

        // Print an error message and the help in the console
        static void PrintError(string error)
        {
            Console.WriteLine("\r\n" + error + "\r\n");
            PrintHelp();
        }

        // Print the help in the console
        static void PrintHelp()
        {
            Console.WriteLine("\r\nUsage: myping [-t] [-n count] [-w timeout]\r\n" +
                "    -t             Ping the specified host until stopped.\r\n" +
                "    -n <count>     Number of echo requests to send.\r\n" +
                "    -l <size>      Buffer size of the message to be send" +
                "    -f             Enable fragment" +
                "    -i <TTL>       Set the TTL (Time To Live) size manually" +
                "    -w <timeout>   Timeout in milliseconds to wait for each reply.\r\n" +
                "                   (0 = no timeout)\r\n" +
                "                   <HOST/IP>\r\n");

        }
    }
}
