//
// IPMessage.cs
// Meleagre.Network
//
// Created by Konstantin Balchikov on 14.03.2013.
// Copyright (c) 2013 Konstantin Balchikov. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Meleagre.Network
{
    public class IPMessage
    {
        public IPMessage() { } 
        public IPMessage(byte[] byteBuffer)
        {
            this.ByteBuffer = byteBuffer;

            byte[] icmpByteMessage = new byte[IcmpMessageSize];
            Array.Copy(
                sourceArray: this.ByteBuffer,
                destinationArray: icmpByteMessage,
                sourceIndex: IPHeaderSize,
                destinationIndex: 0,
                length: IcmpMessageSize);
            icmpMessage = new IcmpMessage(icmpByteMessage);

        }

        public byte[] ByteBuffer { get; private set; }
        
        private IcmpMessage icmpMessage;

        public IcmpMessage IcmpMessage 
        { 
            get { return icmpMessage; }
        }

        public ushort IPHeaderSize
        {
            // Retrieve IPHeader size by getting last 4 bits of the first byte and multiplying it by 4
            get { return (ushort)((ByteBuffer[0] & 0x0F) * 4); }
        }
        
        public ushort TotalMessageSize
        {
            // Retrieve total message size from 3rd and 4th bytes of message, also convertring it
            // from big-endian to little-endian
            get { return (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ByteBuffer, 2)); }
        }

        public ushort IcmpMessageSize
        {
            // Retrieve ICMP message size by substracting IP header size from total size
            get { return (ushort)(TotalMessageSize - IPHeaderSize); }
        }

        public ushort Ttl
        {
            // Retrieve TTL from 9th byte
            get { return (ushort)ByteBuffer[8]; }
        }

        public IPAddress SourceAddress
        {
            // Retrieve source IP address
            get
            {
                byte[] byteIpAddress = new byte[4];
                Array.Copy(
                    sourceArray: ByteBuffer,
                    sourceIndex: 12,
                    destinationArray: byteIpAddress,
                    destinationIndex: 0,
                    length: 4);
                return new IPAddress(byteIpAddress);
            }
        }
        public IPAddress DestinationAddress
        {
            // Retrieve destination IP address
            get
            {
                byte[] byteIpAddress = new byte[4];
                Array.Copy(
                    sourceArray: ByteBuffer,
                    sourceIndex: 16,
                    destinationArray: byteIpAddress,
                    destinationIndex: 0,
                    length: 4);
                return new IPAddress(byteIpAddress);
            }
        }
    }
}
