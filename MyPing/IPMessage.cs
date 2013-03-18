//
// IPMessage.cs
// MyPing
//
// Created by Konstantin Balchikov on 14.03.2013.
// Copyright (c) 2013 Konstantin Balchikov. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace MyPing
{
    public class IPMessage
    {
        public ushort TotalMessageSize { get; private set; }
        public ushort IPHeaderSize { get; private set; }
        public ushort IcmpMessageSize { get; private set; }
        public ushort Ttl { get; private set; }
        public IPAddress SourceAddress { get; private set; }
        public IPAddress DestinationAddress { get; private set; }
        
        public void ReadFromByteBuffer(byte[] byteBuffer)
        {
            byte[] tempIpAddress = new byte[4];
            // Retrieve total message size from 3rd and 4th bytes of message, also convertring it
            // from big-endian to little-endian
            TotalMessageSize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteBuffer, 2));
            // Retrieve IPHeader size by getting last 4 bits of the 1st byte and multiplying by 4 
            // as specified in standard. 
            IPHeaderSize = (ushort)((byteBuffer[0] & 0x0F) * 4);
            // Retrieve TTL from 9th byte
            Ttl = (ushort)byteBuffer[8];
            // Retrieve ICMP message size by substracting IP and ICMP header sizes from total size
            IcmpMessageSize = (ushort)(TotalMessageSize - IPHeaderSize - 8);
            
            // Retrieve source and destination IP Addresses
            Array.Copy(
                sourceArray: byteBuffer,
                sourceIndex: 12,
                destinationArray: tempIpAddress, 
                destinationIndex: 0,
                length: 4);
            SourceAddress = new IPAddress(tempIpAddress);
            Array.Copy(
                sourceArray: byteBuffer,
                sourceIndex: 16,
                destinationArray: tempIpAddress,
                destinationIndex: 0,
                length: 4);
            DestinationAddress = new IPAddress(tempIpAddress);
        }

    }
}
