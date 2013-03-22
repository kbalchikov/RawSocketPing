//
// IcmpMessage.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meleagre.Network
{
    public class IcmpMessage
    {
        public byte Type { get; set; }
        public byte Code { get; set; }
        public ushort CheckSum { get; set; }
        public ushort Identifier { get; set; }
        public ushort SequenceNumber { get; set; }
        public byte[] Data { get; set; }

        public byte[] ByteBuffer { get; set; }

        public IcmpMessage() { }
        
        public IcmpMessage(byte[] byteBuffer)
        {
            this.ByteBuffer = ByteBuffer;
        }

        public byte[] GetBaseHeaderBytes()
        {
            byte[] msg = new byte[4];
            // Fill ICMP Message header with Type, Code and CheckSum
            Array.Copy(
                sourceArray: BitConverter.GetBytes(Type),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 0,
                length: 1);
            Array.Copy(
                sourceArray: BitConverter.GetBytes(Code),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 1,
                length: 1);
            Array.Copy(
                sourceArray: BitConverter.GetBytes(CheckSum),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 2,
                length: 2);

            return msg;
        }

        public byte[] GetInformationMessageBytes()
        {
            byte[] msg = new byte[8];

            Array.Copy(
                sourceArray: GetBaseHeaderBytes(),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 0,
                length: 4);
            Array.Copy(
                sourceArray: BitConverter.GetBytes(Identifier),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 4,
                length: 2);
            Array.Copy(
                sourceArray: BitConverter.GetBytes(SequenceNumber),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 6,
                length: 2);

            return msg;
        }

        // Get Echo message which is used for ping command
        public byte[] GetEchoMessageBytes()
        {
            int length = 8;
            if (Data != null) length += Data.Length;
            byte[] msg = new byte[length];
            Array.Copy(
                sourceArray: GetInformationMessageBytes(),
                sourceIndex: 0,
                destinationArray: msg,
                destinationIndex: 0,
                length: 8);
            if (Data != null)
            {
                Array.Copy(
                    sourceArray: Data,
                    sourceIndex: 0,
                    destinationArray: msg,
                    destinationIndex: 8,
                    length: Data.Length);
            }
            return msg;
        }

        public static ushort CalcCheckSum(IcmpMessage msg)
        {
            ulong sum = 0;
            byte[] bytes = msg.GetEchoMessageBytes();
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
