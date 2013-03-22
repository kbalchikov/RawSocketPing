//
// IPMessage.cs
// MPing
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
        }

        public byte[] ByteBuffer { get; private set; }

        public IcmpMessage IcmpMessage { get; set; }

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
            // Retrieve ICMP message size by substracting IP and ICMP header sizes from total size
            get { return (ushort)(TotalMessageSize - IPHeaderSize - 8); }
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

        //public void ReadFromByteBuffer(byte[] byteBuffer)
        //{
        //    byte[] tempIpAddress = new byte[4];
        //    // Retrieve total message size from 3rd and 4th bytes of message, also convertring it
        //    // from big-endian to little-endian
        //    TotalMessageSize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteBuffer, 2));
        //    // Retrieve IPHeader size by getting last 4 bits of the first byte and multiplying by 4 
        //    // as specified in standard. 
        //    IPHeaderSize = (ushort)((byteBuffer[0] & 0x0F) * 4);
        //    // Retrieve TTL from 9th byte
        //    Ttl = (ushort)byteBuffer[8];
        //    // Retrieve ICMP message size by substracting IP and ICMP header sizes from total size
        //    IcmpMessageSize = (ushort)(TotalMessageSize - IPHeaderSize - 8);
            
        //    // Retrieve source and destination IP Addresses
        //    Array.Copy(
        //        sourceArray: byteBuffer,
        //        sourceIndex: 12,
        //        destinationArray: tempIpAddress, 
        //        destinationIndex: 0,
        //        length: 4);
        //    SourceAddress = new IPAddress(tempIpAddress);
        //    Array.Copy(
        //        sourceArray: byteBuffer,
        //        sourceIndex: 16,
        //        destinationArray: tempIpAddress,
        //        destinationIndex: 0,
        //        length: 4);
        //    DestinationAddress = new IPAddress(tempIpAddress);
        //}

    }
}
