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

            
            TotalMessageSize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteBuffer, 2));
            IPHeaderSize = (ushort)((byteBuffer[0] & 0x0F) * 4);
            Ttl = (ushort)byteBuffer[8];
            IcmpMessageSize = (ushort)(TotalMessageSize - IPHeaderSize - 8);
            
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
