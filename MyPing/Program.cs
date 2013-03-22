//
// Program.cs
// MyPing
//
// Created by Konstantin Balchikov on 14.03.2013.
// Copyright (c) 2013 Konstantin Balchikov. All rights reserved.

//#undef DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using Meleagre.Network;
using CommandLine.Utility;


namespace MPing
{
    class Program
    {
        static short ttl = 128;
        static string host;
        static int sentCount = 4;
        static int timeout = 5000;
        static bool isInfinite = false;
        static ushort buffersize = 32;
        static bool enableFragment = false;
        static int notRecievedAnswersCount = 0;
        static Arguments commandLine;
        static List<TimeSpan> latencies;
        static IPMessage recievedMessage;


        static int Main(string[] args)
        {
            commandLine = new Arguments(args);
            latencies = new List<TimeSpan>();

            if (ParseArguments(args) != 0)
            {
                // Some error occured during parsing
                return 1;
            }
            // Hostname is supposed to be the last command-line argument
            host = args.Last<string>();

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
                    TimeSpan latency;
                    int remainedCount = sentCount;
                    while (isInfinite || remainedCount > 0)
                    {
                        try
                        {
                            latency = client.Ping(timeout, buffersize, !enableFragment, ttl, out recievedMessage);
                            if (latency.Equals(TimeSpan.MaxValue))
                            {
                                notRecievedAnswersCount++;
                                Console.WriteLine("Request timed out.");
                            }
                            else
                            {
                                latencies.Add(latency);
                                Console.WriteLine("Reply from {0}: bytes={1}, time={2} ms, TTL={3}",
                                    recievedMessage.SourceAddress,
                                    recievedMessage.IcmpMessageSize,
                                    Math.Round(latency.TotalMilliseconds),
                                    recievedMessage.Ttl);
                            }

                            //If latency is less than 1 second then wait until it passes.
                            if (latency.TotalMilliseconds < 1000)
                            {
                                System.Threading.Thread.Sleep(1000 - (int)latency.TotalMilliseconds);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Network error: " + e.Message);
                            Debug.WriteLine(e.ToString());
                        }
                        if (!isInfinite) remainedCount--;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error while pinging the specified host.");
            }

            PrintStatistics(hostIP, sentCount, notRecievedAnswersCount, latencies);
            #if DEBUG
                Console.ReadLine();
            #endif
            return 0;
        }

        // Print an error message and the help in the console
        static void PrintError(string error)
        {
            Console.WriteLine("\r\n" + error + "\r\n");
            PrintHelp();
        }

        static void PrintStatistics(IPAddress host, int totalCount, int notRecieved, List<TimeSpan> latencies)
        {
            Console.WriteLine("\r\nStatistics ping for {0}:\n\tPackets: sent = {1}, recieved = {2}, lost = {3}", 
                host, totalCount, totalCount - notRecieved, notRecieved);
            Console.WriteLine("\t({0}% lost)", Math.Round((double)notRecieved / totalCount * 100));
            // Print time data only if at least one answer was recieved
            if (latencies.Count > 0)
            {
                Console.WriteLine("Approximate time:");
                Console.WriteLine("\tMinimal = {0} ms, Maximal = {1} ms, Average = {2} ms",
                        (int)latencies.Min<TimeSpan>().TotalMilliseconds,
                        (int)latencies.Max<TimeSpan>().TotalMilliseconds,
                        (int)latencies.Average<TimeSpan>(timeSpan => timeSpan.TotalMilliseconds)); 
            }
        }

        static int ParseArguments(string[] args)
        {
            // Showing help is there are no parameters of -? (/?) parameter is met
            if (args.Length == 0 || commandLine["?"] != null)
            {
                PrintHelp();
                #if DEBUG
                Console.ReadLine();
                #endif
                return 0;
            }

            if (commandLine["n"] != null)
                if (!int.TryParse(commandLine["n"], out sentCount))
                {
                    PrintError("Invalid argument for option -n");
                    return 1;
                }

            if (commandLine["t"] != null)
                isInfinite = true;

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

            return 0;
        }

        // Print the help in the console
        static void PrintHelp()
        {
            Console.WriteLine(
                "\r\nUsage: myping [-t] [-n <count>] [-w <timeout>] [-l <size>]\r\n" +
                "              [-f] [-i <TTL>] <HOST/IP>\r\n" + 
                "    -t             Ping the specified host until stopped.\r\n" +
                "    -n <count>     Number of echo requests to send.\r\n" +
                "    -l <size>      Buffer size of the message to be send\n" +
                "    -f             Enable fragment\n" +
                "    -i <TTL>       Set the TTL (Time To Live) size manually\n" +
                "    -w <timeout>   Timeout in milliseconds to wait for each reply.\r\n" +
                "                   (0 = no timeout)\r\n");

        }
    }
}
