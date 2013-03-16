using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPing
{
    public class IcmpMessage
    {
        public byte Type { get; set; }
        public byte Code { get; set; }
        public ushort CheckSum { get; set; }
        public ushort Identifier { get; set; }
        public ushort SequenceNumber { get; set; }
        public byte[] Data { get; set; }

        public byte[] GetBaseHeaderBytes()
        {
            byte[] msg = new byte[4];
            

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
