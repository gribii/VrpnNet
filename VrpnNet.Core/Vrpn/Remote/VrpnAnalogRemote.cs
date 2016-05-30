using System;
using System.Linq;

namespace VrpnNet.Core.Vrpn.Remote
{
    /// <summary>
    ///     Remote handler for supporting the analog class.
    /// </summary>
    public class VrpnAnalogRemote : VrpnRemoteBase
    {
        /// <summary>
        ///     Handler for &quot;Analog Channel&quot; message.
        /// </summary>
        public delegate void ChannelMessage(VrpnMessageHeader header, ChannelData data);

        /// <summary>
        ///     Event raised on new channel message arrived.
        /// </summary>
        public event ChannelMessage ChannelReceived;

        /// <summary>
        ///     Convert the raw data into an object.
        /// </summary>
        /// <param name="msg">Message containing headers and raw data.</param>
        private void HandleAnalogChannel(VrpnMessage msg)
        {
            var data = new ChannelData
            {
                Channels =
                    new double[(int) BitConverter.ToDouble(msg.Payload.Take(sizeof (double)).Reverse().ToArray(), 0)]
            };
            var offset = sizeof (double);
            for (var i = 0; i < data.Channels.Length; i++)
            {
                data.Channels[i] =
                    BitConverter.ToDouble(msg.Payload.Skip(offset).Take(sizeof (double)).Reverse().ToArray(), 0);
                offset += sizeof (double);
            }

            this.ChannelReceived?.Invoke(msg.Header, data);
        }

        public override void RegisterTypes()
        {
            TypeRegistration.Instance.RegisterLocalType("vrpn_Analog Channel", this.HandleAnalogChannel);
        }

        /// <summary>
        /// Parsed data
        /// </summary>
        public class ChannelData
        {
            public double[] Channels;
        }
    }
}