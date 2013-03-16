using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;


namespace MyPing
{
    public class IcmpClient : IDisposable
    {
        public IPAddress Host { get; set; }
        public Socket Client { get; set; }

        private IcmpMessage icmpMessage;
        private Timer pingTimeOut;
        private bool hasTimedOut;

        public IcmpClient(IPAddress host)
        {
            this.Host = host;
        }

        public IcmpClient(string hostname)
        {
            this.Host = Dns.GetHostAddresses(hostname)[0];
        }

        private byte[] GetEchoMessage(ushort bufferSize)
        {
            icmpMessage = new IcmpMessage();
            icmpMessage.Type = 8;
            icmpMessage.Data = new byte[bufferSize];
            
            for (int i = 0; i < bufferSize; i++)
                icmpMessage.Data[i] = 32;
            
            icmpMessage.CheckSum = IcmpMessage.CalcCheckSum(icmpMessage);
            return icmpMessage.GetEchoMessageBytes();
        }

        public TimeSpan Ping(out IPMessage ipMessage)
        {
            return Ping(timeout: 1000, bufferSize: 32, dontFragment: true, ttl: 128, ipMessage: out ipMessage);
        }

        public TimeSpan Ping(int timeout, out IPMessage ipMessage)
        {
            return Ping(timeout: timeout, bufferSize: 32, dontFragment: true, ttl: 128, ipMessage: out ipMessage);
        }

        public TimeSpan Ping(int timeout, ushort bufferSize, bool dontFragment, short ttl, out IPMessage ipMessage)
        {
            ipMessage = new IPMessage();
            TimeSpan latency;
            EndPoint endPoint = new IPEndPoint(Host, 0);

            Client = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            // Getting message to send
            byte[] message = GetEchoMessage(bufferSize);            
            // Setting fragmentation
            Client.DontFragment = dontFragment;
            // Setting TTL
            Client.Ttl = ttl;

            hasTimedOut = false;
            pingTimeOut = new Timer(
                callback: new TimerCallback(OnPingTimedOut), 
                state: null, 
                dueTime: timeout, 
                period: System.Threading.Timeout.Infinite);

            DateTime startTime = DateTime.Now;
            try
            {
                if (Client.SendTo(message, endPoint) <= 0)
                    throw new SocketException();
                message = new byte[message.Length + 20];
                if (Client.ReceiveFrom(message, ref endPoint) <= 0)
                    throw new SocketException();
                
            }
            catch (SocketException e)
            {
                if (hasTimedOut) return TimeSpan.MaxValue;
                else throw e;
            }
            finally
            {
                Client.Close();
                Client = null;
                pingTimeOut.Change(
                    System.Threading.Timeout.Infinite, 
                    System.Threading.Timeout.Infinite);
                pingTimeOut.Dispose();
                pingTimeOut = null;
            }

            latency = DateTime.Now.Subtract(startTime);
            ipMessage.ReadFromByteBuffer(message);
            return latency;
        }

        private void OnPingTimedOut(object state)
        {
            hasTimedOut = true;
            if (Client != null) Client.Close();
        }

        void IDisposable.Dispose()
        {
            if (Client != null)
            {
                Client.Close();
                Client.Dispose();
            }
            
        }
    }
}
