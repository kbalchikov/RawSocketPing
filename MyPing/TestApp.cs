using System;
using System.Net;
using System.Threading;
using System.Collections;
using Org.Mentalis.Network;
using System.Diagnostics;

class TestApp {
    //static void Main(string[] args)
    //{
    //    TestApp testApp = new TestApp();
    //    testApp.Start(args);
    //}
    public void Start(string[] args) {

        args = new string[] { "vk.com" };

        string host;
        int count = 4;
        int timeout = 1000;
        bool infinite = false;
        string current;
        if (args != null && args.Length > 0) {
            // Parse the arguments
            Queue queue = new Queue(args);
            host = (string)queue.Dequeue();
            while (queue.Count > 0) {
                current	= (string)queue.Dequeue();
                switch(current.ToLower()) {
                    case "-t":
                        infinite = true;
                        break;
                    case "-n":
                        if (queue.Count == 0) {
                            PrintError("Argument required for option '" + current + "'");
                            return;
                        } else {
                            try {
                                count = int.Parse((string)queue.Dequeue());
                                if (count <= 0)
                                    throw new Exception();
                            } catch {
                                PrintError("Invalid argument for option '" + current + "'");
                                return;
                            }
                        }
                        break;
                    case "-w":
                        if (queue.Count == 0) {
                            PrintError("Argument required for option '" + current + "'");
                            return;
                        } else {
                            try {
                                timeout = int.Parse((string)queue.Dequeue());
                                if (timeout < 0)
                                    throw new Exception();
                                if (timeout == 0)
                                    timeout = Timeout.Infinite;
                            } catch {
                                PrintError("Invalid argument for option '" + current + "'");
                                return;
                            }
                        }
                        break;
                    default:
                        PrintError("Invalid option '" + current + "'");
                        return;
                }
            } 
            // Resolve the specified IP address
            IPAddress hostIP;
            try {
                hostIP = Dns.GetHostAddresses(host)[0];
            } catch {
                Console.WriteLine("Unable to resolve the specified hostname.");
                return;
            }
            // Start pinging
            try {
                if (host.Equals(hostIP.ToString()))
                    Console.WriteLine("\r\nPinging " + host + " with 32 bytes of data:\r\n");
                else
                    Console.WriteLine("\r\nPinging " + host + " [" + hostIP.ToString() + "] with 32 bytes of data:\r\n");
                Icmp icmp = new Icmp(hostIP);
                TimeSpan ret;
                while(infinite || count > 0) {
                    try {
                        ret = icmp.Ping(timeout);
                        if (ret.Equals(TimeSpan.MaxValue))
                            Console.WriteLine("Request timed out.");
                        else
                            Console.WriteLine("Reply from " + hostIP.ToString() + ": bytes=32 time=" + Math.Round(ret.TotalMilliseconds).ToString() + "ms");
                        if (1000 - ret.TotalMilliseconds > 0)
                            Thread.Sleep(1000 - (int)ret.TotalMilliseconds);
                    } catch {
                        Console.WriteLine("Network error.");
                    }
                    if (!infinite)
                        count--;
                }
            } catch {
                Console.WriteLine("Error while pinging the specified host.");
                return;
            }
        } else { // no parameters specified
            PrintHelp();
        }

        Console.ReadLine();
    }
    // Print an error message and the help in the console
    protected void PrintError(string error) {
        Console.WriteLine("\r\n" + error + "\r\n");
        PrintHelp();
    }
    // Print the help in the console
    protected void PrintHelp() {
        Console.WriteLine("\r\nUsage: icmp <HOST/IP> [-t] [-n count] [-w timeout]\r\n" + 
            "    -t             Ping the specified host until stopped.\r\n" +
            "    -n count       Number of echo requests to send.\r\n" +
            "    -w timeout     Timeout in milliseconds to wait for each reply.\r\n" +
            "                   (0 = no timeout)\r\n");
    }
}