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


namespace Meleagre.Network
{
    public class IcmpClient : IDisposable
    {
        public Socket Socket { get; set; }
        public IPEndPoint IpEndPoint { get; private set; }

        private IcmpMessage icmpMessage;
        private Timer pingTimeOut;
        private bool hasTimedOut;

        public IcmpClient(IPEndPoint ipEndPoint)
        {
            this.IpEndPoint = ipEndPoint;
        }

        public IcmpClient(IPAddress host) :
            this(new IPEndPoint(host, 0)) { }

        public IcmpClient(string hostname) : 
            this(Dns.GetHostAddresses(hostname)[0]) { }

        private byte[] GetEchoMessage(ushort bufferSize)
        {
            icmpMessage = new IcmpMessage();
            // Type 8 is for echo request
            icmpMessage.Type = 8;
            // Just some identifier (can be changed to identify incoming replies or something)
            icmpMessage.Identifier = 0x100;
            icmpMessage.Data = new byte[bufferSize];

            // Kind of an easter egg here: we can send any message we like with our ICMP datagram
            // If the request reached its destination, data block normally would be returned without changes.
            byte[] asciiMessage = System.Text.Encoding.ASCII.GetBytes("Having fun with ICMP");
            // Our message is sent only if it fits in required buffer size
            if (asciiMessage.Length <= bufferSize)
            {
                Array.Copy(
                    sourceArray: asciiMessage,
                    sourceIndex: 0,
                    destinationArray: icmpMessage.Data,
                    destinationIndex: 0,
                    length: asciiMessage.Length
                    );
                // The remained data we are filling with spaces (which is 32 in ASCII)
                for (int i = asciiMessage.Length; i < bufferSize; i++)
                    icmpMessage.Data[i] = 32;
            }
            else // we just send spaces
            {
                for (int i = 0; i < bufferSize; i++)
                    icmpMessage.Data[i] = 32;
            }
            
            icmpMessage.CheckSum = IcmpMessage.CalcCheckSum(icmpMessage);
            return icmpMessage.GetEchoMessageBytes();
        }

        public TimeSpan Ping(out IPMessage ipMessage)
        {
            return Ping(timeout: 1000, bufferSize: 32, 
                dontFragment: true, ttl: 128, ipMessage: out ipMessage);
        }

        public TimeSpan Ping(int timeout, out IPMessage ipMessage)
        {
            return Ping(timeout: timeout, bufferSize: 32, 
                dontFragment: true, ttl: 128, ipMessage: out ipMessage);
        }

        public TimeSpan Ping(int timeout, ushort bufferSize, bool dontFragment, short ttl, out IPMessage ipMessage)
        {
            Socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Raw,
                ProtocolType.Icmp);

            //ipMessage = new IPMessage();
            TimeSpan latency;
            EndPoint endPoint = this.IpEndPoint;

            // Getting message to send
            byte[] message = GetEchoMessage(bufferSize);            
            // Setting fragmentation
            Socket.DontFragment = dontFragment;
            // Setting TTL
            Socket.Ttl = ttl;

            hasTimedOut = false;
            pingTimeOut = new Timer(
                callback: new TimerCallback(OnPingTimedOut), 
                state: null, 
                dueTime: timeout, 
                period: System.Threading.Timeout.Infinite);

            DateTime startTime = DateTime.Now;
            try
            {
                if (Socket.SendTo(message, endPoint) <= 0)
                    throw new SocketException();
                message = new byte[message.Length + 200];
                if (Socket.ReceiveFrom(message, ref endPoint) <= 0)
                    throw new SocketException();
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
                if (Socket != null)
                {
                    Socket.Close();
                    Socket = null; 
                }
                
                ipMessage = new IPMessage(message);
            }

            latency = DateTime.Now.Subtract(startTime);
            return latency;
        }

        private void OnPingTimedOut(object state)
        {
            hasTimedOut = true;
            if (Socket != null)
            {
                Socket.Close();
                Socket = null;
            }
        }

        void IDisposable.Dispose()
        {
            if (Socket != null)
            {
                Socket.Close();
                Socket.Dispose();
            }
            
        }
    }
}
