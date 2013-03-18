//
// IcmpClient.cs
// MyPing
//
// Created by The KPD-Team on 15.04.2002.
// Copyright (c) 2013 2002, The KPD-Team. All rights reserved.
// http://www.mentalis.org/
// 
// Modified by Konstantin Balchikov on 14.03.2013
/*

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions
  are met:

    - Redistributions of source code must retain the above copyright
       notice, this list of conditions and the following disclaimer. 

    - Neither the name of the KPD-Team, nor the names of its contributors
       may be used to endorse or promote products derived from this
       software without specific prior written permission. 

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
  THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
  STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
  OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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
                message = new byte[message.Length + 100];
                //if (Client.ReceiveFrom(message, ref endPoint) <= 0)
                //    throw new SocketException();
                Client.ReceiveFrom(message, ref endPoint);
                
            }
            catch (SocketException e)
            {
                if (hasTimedOut) return TimeSpan.MaxValue;
                else throw e;
            }
            finally
            {
                pingTimeOut.Change(
                    System.Threading.Timeout.Infinite, 
                    System.Threading.Timeout.Infinite);
                pingTimeOut.Dispose();
                pingTimeOut = null;
                if (Client != null)
                {
                    Client.Close();
                    Client = null; 
                }
            }

            latency = DateTime.Now.Subtract(startTime);
            ipMessage.ReadFromByteBuffer(message);
            return latency;
        }

        private void OnPingTimedOut(object state)
        {
            hasTimedOut = true;
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
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
