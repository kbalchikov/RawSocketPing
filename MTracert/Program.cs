//
// Program.cs
// MTracert
//
// Created by Konstantin Balchikov on 22.03.2013.
// Copyright (c) 2013 Konstantin Balchikov. All rights reserved.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meleagre.Network;
using CommandLine.Utility;
using System.Net;


namespace MTracert
{
    class Program
    {
        static string hostname;
        static int maxHops = 30;
        static int timeout = 1000;
        static ushort buffersize = 32;
        static int pingAttemts = 3;
        static Arguments commandLine;

        const int port = 33434;

        static int Main(string[] args)
        {
            commandLine = new Arguments(args);
            if (!TryParseArguments(args))
            {
                return 1;
            }
            hostname = args.Last();
            
            IPAddress hostIP;
            try
            {
                hostIP = Dns.GetHostAddresses(hostname)[0];
            }
            catch
            {
                Console.WriteLine("Unable to resolve specified hostname.");
                return 1;
            }

            IPEndPoint ipEndPoint = new IPEndPoint(hostIP, port);
            try
            {
                if (hostname.Equals(hostIP.ToString()))
                    Console.WriteLine("\r\nTracing to {0} with {1} bytes of data:\r\n",
                        hostname, buffersize.ToString());
                else Console.WriteLine("\r\nTracing to {0} [{1}] with {2} bytes of data:\r\n",
                    hostname, hostIP.ToString(), buffersize.ToString());

                using (IcmpClient client = new IcmpClient(ipEndPoint))
                {
                    short ttl = 1;
                    IPMessage ipMessage = new IPMessage();
                    while(true)
                    {
                        List<TimeSpan> latencies = new List<TimeSpan>();
                        for (int i = 0; i < pingAttemts; i++)
                        {
                            TimeSpan latency = client.Ping(timeout, buffersize, true, ttl, out ipMessage);
                            latencies.Add(latency);

                        }
                        if (latencies.All(l => l.Equals(TimeSpan.MaxValue)))
                        {
                            Console.WriteLine("1\tRequest timed out.");
                        }
                        else
                        {
                            latencies = latencies.FindAll(l => !l.Equals(TimeSpan.MaxValue));
                            Console.WriteLine("\r\n1\t{0} ms\t{1} ms\t{2} ms\t{3}",
                                (int)latencies.Min<TimeSpan>().TotalMilliseconds,
                                (int)latencies.Max<TimeSpan>().TotalMilliseconds,
                                (int)latencies.Average<TimeSpan>(timeSpan => timeSpan.TotalMilliseconds),
                                ipMessage.SourceAddress);
                        }
                        ttl++;
                    }

                }
            }
            catch
            {

            }

            Console.WriteLine("Hostname: {0}, h parameter: {1}, w parameter: {2}",
                hostname, commandLine["h"], commandLine["w"]);
            Console.ReadLine();

            return 0;
        }

        static bool TryParseArguments(string[] args)
        {
            if (args.Length == 0 || commandLine["?"] != null)
            {
                PrintHelp();
#if DEBUG
                Console.ReadLine();
#endif
                return false;
            }

            if (commandLine["h"] != null)
            {
                if (!int.TryParse(commandLine["h"], out maxHops))
                {
                    PrintError("Invalid argument for option -h");
                    return false; 
                }
            }

            if (commandLine["w"] != null)
            {
                if (!int.TryParse(commandLine["w"], out timeout))
                {
                    PrintError("Invalid argument for option -w");
                    return false;
                }
            }

            return true;
        }

        static void PrintHelp()
        {
            Console.WriteLine("Help");
        }

        static void PrintError(string error)
        {
            Console.WriteLine("\r\n" + error + "\r\n");
            PrintHelp();
        }
    }
}
