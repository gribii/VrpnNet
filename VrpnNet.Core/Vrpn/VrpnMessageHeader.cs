using System;
using System.Linq;
using VrpnNet.Core.Utils;

namespace VrpnNet.Core.Vrpn
{
    /// <summary>
    ///     Represents the header data in a VRPN message sent over network.
    /// </summary>
    public class VrpnMessageHeader
    {
        private VrpnMessageHeader(uint length, uint ceiledLength, int sender, int type)
        {
            this.Length = length;
            this.CeiledLength = ceiledLength;
            this.Sender = sender;
            this.Type = type;
        }

        /// <summary>
        ///     The real length of the message.
        /// </summary>
        public uint Length { get; }

        /// <summary>
        ///     The length of the message using <see cref="VrpnConstants.VRPN_ALIGN" />.
        /// </summary>
        public uint CeiledLength { get; private set; }

        /// <summary>
        ///     The length of the header data in bytes.
        /// </summary>
        public static uint HeaderLength => VrpnConstants.VRPN_HEADER_LENGTH;

        /// <summary>
        ///     ID of the sender of this message.
        /// </summary>
        public int Sender { get; }

        /// <summary>
        ///     Message type
        /// </summary>
        public int Type { get; }

        /// <summary>
        ///     Parse header data received over network into an object.
        /// </summary>
        /// <param name="buffer">The buffer where the raw header data are stored.</param>
        /// <returns>A new message header instance.</returns>
        public static VrpnMessageHeader Parse(byte[] buffer)
        {
            var len = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0) - 24;
            var ceil_len = len;
            if (len%8 != 0)
            {
                ceil_len += 8 - len%8;
            }
            var sender = BitConverter.ToInt32(buffer.Skip(12).Take(4).Reverse().ToArray(), 0);
            var type = BitConverter.ToInt32(buffer.Skip(16).Take(4).Reverse().ToArray(), 0);

            return new VrpnMessageHeader(len, ceil_len, sender, type);
        }

        /// <summary>
        ///     Create a new message header based on the raw data which should be sent.
        /// </summary>
        /// <param name="payload">The raw data of the message.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <returns></returns>
        public static VrpnMessageHeader Create(byte[] payload, int sender, int type)
        {
            var len = (uint) payload.Length;
            var ceil_len = len;
            if (ceil_len%VrpnConstants.VRPN_ALIGN != 0)
                ceil_len += VrpnConstants.VRPN_ALIGN - ceil_len%VrpnConstants.VRPN_ALIGN;
            return new VrpnMessageHeader(len, ceil_len, sender, type);
        }

        /// <summary>
        ///     Pack the data from this instance into a raw buffer which can be sent over network.
        /// </summary>
        /// <returns>The packed data.</returns>
        public byte[] Pack()
        {
            var header1 = NetUtils.htonl(VrpnMessageHeader.HeaderLength + this.Length);
            var header2 = NetUtils.htonl(0);
            var header3 = NetUtils.htonl(0);
            var header4 = NetUtils.htonl(this.Sender);
            var header5 = NetUtils.htonl(this.Type);

            var header = new byte[VrpnMessageHeader.HeaderLength];
            var offset = 0;
            
            for (var i = 0; i < 4; i++) header[offset++] = header1[i];
            for (var i = 0; i < 4; i++) header[offset++] = header2[i];
            for (var i = 0; i < 4; i++) header[offset++] = header3[i];
            for (var i = 0; i < 4; i++) header[offset++] = header4[i];
            for (var i = 0; i < 4; i++) header[offset++] = header5[i];

            return header;
        }
    }
}