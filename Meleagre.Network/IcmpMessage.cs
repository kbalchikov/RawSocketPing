//
// IcmpMessage.cs
// Meleagre.Network
//
// Created by Konstantin Balchikov on 14.03.2013.
// Copyright (c) 2013 Konstantin Balchikov. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meleagre.Network
{
    public class IcmpMessage
    {
        // ICMP message format:
        // 0x00 Type
        // 0x01 Code
        // 0x02 CheckSum
        // 0x04 Identifier
        // 0x06 SequenceNumber
        // 0x08 Data

        // Stores ICMP Header size in bytes
        public const int IcmpHeaderSize = 8;
        
        // Byte array that for header
        public byte[] HeaderBytes { get; private set; }
        
        public byte[] Data
        {
            get { return data; }
            set 
            {
                // Data buffer is not allowed to be larger than 65535 bytes
                if (value.Length > 65535) 
                    throw new ArgumentOutOfRangeException(); 
                data = value; 
            }
        }

        public byte Type
        {
            get { return HeaderBytes[0]; }
            set { HeaderBytes[0] = value; }
        }
        
        public byte Code
        {
            get { return HeaderBytes[1]; }
            set { HeaderBytes[1] = value; }
        }
        
        public ushort CheckSum 
        {
            get { return BitConverter.ToUInt16(HeaderBytes, 2); }
            set 
            { 
                Array.Copy(
                    sourceArray: BitConverter.GetBytes(value), 
                    sourceIndex: 0, 
                    destinationArray: HeaderBytes, 
                    destinationIndex: 2, 
                    length: 2); 
            }
        }
        
        public ushort Identifier
        {
            get { return BitConverter.ToUInt16(HeaderBytes, 4); }
            set
            {
                Array.Copy(
                    sourceArray: BitConverter.GetBytes(value),
                    sourceIndex: 0,
                    destinationArray: HeaderBytes,
                    destinationIndex: 4,
                    length: 2);
            }
        }
        
        public ushort SequenceNumber
        {
            get { return BitConverter.ToUInt16(HeaderBytes, 6); }
            set
            {
                Array.Copy(
                    sourceArray: BitConverter.GetBytes(value),
                    sourceIndex: 0,
                    destinationArray: HeaderBytes,
                    destinationIndex: 6,
                    length: 2);
            }
        }

        public byte[] GetMessageBytes()
        {
            byte[] msg;

            if (Data == null)
            {
                msg = new byte[IcmpHeaderSize];
                Array.Copy(HeaderBytes, msg, IcmpHeaderSize);
            }
            else
            {
                msg = new byte[IcmpHeaderSize + Data.Length];
                Array.Copy(HeaderBytes, msg, IcmpHeaderSize);
                Array.Copy(
                    sourceArray: Data,
                    sourceIndex: 0,
                    destinationArray: msg,
                    destinationIndex: IcmpHeaderSize,
                    length: Data.Length);
            }
            return msg;
        }

        private byte[] data;

        public IcmpMessage()
        {
            HeaderBytes = new byte[IcmpHeaderSize];
        }

        public IcmpMessage(byte[] byteBuffer) : this()
        {
            Array.Copy(
                sourceArray: byteBuffer,
                destinationArray: HeaderBytes,
                sourceIndex: 0,
                destinationIndex: 0,
                length: IcmpHeaderSize);

            data = new byte[byteBuffer.Length - IcmpHeaderSize];
            Array.Copy(
                sourceArray: byteBuffer,
                destinationArray: data,
                sourceIndex: IcmpHeaderSize,
                destinationIndex: 0,
                length: byteBuffer.Length - IcmpHeaderSize);
        }

        public static ushort CalcCheckSum(IcmpMessage msg)
        {
            ulong sum = 0;
            byte[] bytes = msg.GetMessageBytes();
            int i;
            for (i = 0; i < bytes.Length - 1; i += 2)
                sum += BitConverter.ToUInt16(bytes, i);
            if (i != bytes.Length) sum += bytes[i];

            sum = (sum >> 16) + (sum & 0xFFFF);
            sum += (sum >> 16);

            return (ushort)(~sum);   
        }

    }
}
