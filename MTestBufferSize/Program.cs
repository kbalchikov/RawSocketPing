using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Meleagre.Network;
using CommandLine.Utility;


namespace MTU
{
    class Program
    {
        static string hostname;
        static int timeout = 3000;
        static int pingAttempts = 1;
        static short ttl = 128;
        static bool enableFragment = false;
        static Arguments commandLine;

        static int Main(string[] args)
        {
            commandLine = new Arguments(args);

            if (args.Length == 0)
            {
                PrintError("Please, specify one host or IP you'd like to test.");
                return 1;
            }

            if (commandLine["f"] != null)
            {
                enableFragment = true;
            }

            hostname = args.Last();

            IPAddress hostIP;
            try
            {
                hostIP = Dns.GetHostAddresses(hostname)[0];
            }
            catch
            {
                PrintError("Unable to resolve specified hostname.");
                return 1;
            }
            IPEndPoint ipEndPoint = new IPEndPoint(hostIP, 0);
            //if (hostname.Equals(hostIP.ToString()))
            //    Console.WriteLine("\r\nChecking maximum buffer size\nfor incoming not fragmented packets of {0}", hostname);
            //else Console.WriteLine("\r\nChecking maximum buffer size\nfor incoming not fragmented packets of {0} [{1}]",
            //    hostname, hostIP.ToString());

            using (IcmpClient client = new IcmpClient(ipEndPoint))
            {
                IPMessage ipMessage = new IPMessage();
                // First we try to ping host with no buffer data several times
                // just to make sure if host is available
                TimeSpan latency;
                bool isAvailable = false;
                for (int i = 0; i < pingAttempts && !isAvailable; i++)
                {
                    try
                    {
                        latency = client.Ping(
                            timeout: timeout, bufferSize: 0,
                            dontFragment: !enableFragment, ttl: ttl, ipMessage: out ipMessage);
                        // If latency is not <TimeSpan.MaxValue> (which symbolizes request timed out)
                        // or if returned message does not contain "TTL exceeded in transit" error
                        // then we assume this host as available
                        if (latency != TimeSpan.MaxValue && ipMessage.IcmpMessage.Type != 11)
                            isAvailable = true;
                    }
                    catch (Exception e)
                    {
                        PrintError(e.Message);
                        return 1;
                    }
                }

                if (!isAvailable)
                {
                    PrintError("Destination host is unavailable: request timed out.");
                    return 1;
                }
                Console.WriteLine("\nHost is available.\n");

                ushort max = ushort.MaxValue, min = 0, mid = 0;
                while (min < max)
                {
                    mid = (ushort)(min + (max - min) / 2);
                    if (TryPing(mid, client))
                    {
                        min = (ushort)(mid + 1);
                        Console.WriteLine("Pinging with \t{0}\t bytes of data : +", mid + 28);
                    }
                    else
                    {
                        max = mid;
                        Console.WriteLine("Pinging with \t{0}\t bytes of data : -", mid + 28);
                    }
                }
                Console.WriteLine("\nMax buffer size is: {0} bytes\n", mid + 28);
            }
            return 0;
        }

        // Returns true if pinging was successful, 
        // otherwise - false
        static bool TryPing(ushort s, IcmpClient client)
        {
            TimeSpan latency;
            IPMessage ipMessage = new IPMessage();
            for (int i = 0; i < pingAttempts; i++)
            {
                try
                {
                    latency = client.Ping(timeout: timeout, bufferSize: s,
                        dontFragment: !enableFragment, ttl: ttl, ipMessage: out ipMessage);
                    if (latency != TimeSpan.MaxValue && ipMessage.IcmpMessage.Type != 11)
                        return true;
                }
                catch { }
            }
            return false;
        }

        static void PrintError(string error)
        {
            Console.WriteLine(error);
        }
    }
}
