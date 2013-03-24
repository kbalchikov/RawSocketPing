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
using System.Net;
using System.Net.Sockets;
using Meleagre.Network;
using CommandLine.Utility;


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

            // If parsing failed then return error code 1
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
                    Console.WriteLine("\r\nTracing to {0} with {1} bytes of data",
                        hostname, buffersize.ToString());
                else Console.WriteLine("\r\nTracing to {0} [{1}] with {2} bytes of data",
                    hostname, hostIP.ToString(), buffersize.ToString());
                Console.WriteLine("maximum hops: {0}\r\n", maxHops);
                
                using (IcmpClient client = new IcmpClient(ipEndPoint))
                {
                    short ttl = 1;
                    IPMessage ipMessage = new IPMessage();
                    // Type of returned ICMP message. 0 is echo-reply, 
                    // 11 is Time To Live exceeded
                    int replyIcmpType = -1;
                    while (replyIcmpType != 0 && ttl <= maxHops)
                    {
                        // Create list for ping times to current node
                        List<TimeSpan> latencies = new List<TimeSpan>();
                        for (int i = 0; i < pingAttemts; i++)
                        {
                            // Pinging current node <pingAttempts> times
                            TimeSpan latency = client.Ping(timeout, buffersize, true, ttl, out ipMessage);
                            latencies.Add(latency);
                        }
                        // If all latencies are <TimeSpan.MaxValue>, then this host is unavailable
                        if (latencies.All(l => l.Equals(TimeSpan.MaxValue)))
                        {
                            Console.WriteLine("{0}\tRequest timed out.", ttl);
                        }
                        else
                        {
                            // Select all latencies except ones that are <TimeSpan.MaxValue>
                            latencies = latencies.FindAll(l => !l.Equals(TimeSpan.MaxValue));
                            Console.Write("{0}\t{1} ms\t{2} ms\t{3} ms\t", ttl,
                                (int)latencies.Min<TimeSpan>().TotalMilliseconds,
                                (int)latencies.Max<TimeSpan>().TotalMilliseconds,
                                (int)latencies.Average<TimeSpan>(t => t.TotalMilliseconds));

                            // Here we are trying to get host name of current node if it's possible
                            IPHostEntry hostEntry = new IPHostEntry();
                            try
                            {
                                hostEntry = Dns.GetHostEntry(ipMessage.SourceAddress);
                            }
                            catch (SocketException) { }

                            if (hostEntry.HostName != null) 
                                Console.WriteLine("{0} [{1}]", hostEntry.HostName, ipMessage.SourceAddress);
                            else Console.WriteLine(ipMessage.SourceAddress.ToString());

                            // Saving ICMP message type to decide if we continue loop or not
                            replyIcmpType = ipMessage.IcmpMessage.Type;
                        }
                        // Increment TTL in order to achieve further node
                        ttl++;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Some error occured while tracing the specified host.");

            }
            Console.WriteLine("\nTracing completed.");
            Console.ReadLine();

            return 0;
        }
        
        // Method for parsing command-line arguments.
        // Returns true if parsing was successful, otherwise false
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
            Console.WriteLine(
                "\r\nUsage: mtracert [-h <hops>] [-w <timeout>] <HOST/IP>\r\n" +

                "    -h <hops>      Maximum amount of hops.\r\n" +
                "    -w <timeout>   Timeout in milliseconds to wait for each reply.\r\n" +
                "                   (0 = no timeout)\r\n");
        }

        static void PrintError(string error)
        {
            Console.WriteLine("\r\n" + error + "\r\n");
            PrintHelp();
        }
    }
}
