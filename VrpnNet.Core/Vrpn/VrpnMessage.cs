using System.Linq;
using System.Net.Sockets;
using System.Text;
using VrpnNet.Core.Utils;

namespace VrpnNet.Core.Vrpn
{
    /// <summary>
    ///     Represents a message object for data sent over network.
    /// </summary>
    public class VrpnMessage
    {
        /// <summary>
        ///     Initialize a new instance.
        /// </summary>
        /// <param name="header">The parsed header.</param>
        /// <param name="payload">The raw body data of the message.</param>
        public VrpnMessage(VrpnMessageHeader header, byte[] payload)
        {
            this.Header = header;
            this.Payload = payload;
        }

        /// <summary>
        ///     Header of this message.
        /// </summary>
        public VrpnMessageHeader Header { get; }

        /// <summary>
        ///     Raw body data of this message.
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        ///     Send this message over the network.
        /// </summary>
        /// <param name="s">The socket where to send data over.</param>
        public void Send(Socket s)
        {
            var h = this.Header.Pack();
            var data = new byte[h.Length + this.Header.CeiledLength];
            h.CopyTo(data, 0);
            this.Payload.CopyTo(data, h.Length);

            s.Send(data);
        }

        /// <summary>
        ///     Receive a message from the network.
        /// </summary>
        /// <param name="s">The socket where to receive data from.</param>
        /// <param name="udp">
        ///     Specify if it is a TCP or a UDP socket. This information is important because buffer sizes have to be
        ///     assigned in a different way.
        /// </param>
        /// <returns>The message received over network.</returns>
        public static VrpnMessage Receive(Socket s, bool udp)
        {
            var buffer = new byte[VrpnMessageHeader.HeaderLength];
            if (udp) buffer = new byte[VrpnConstants.VRPN_UDP_BUFFER_LENGTH];

            s.Receive(buffer);

            // parse the header
            var header = VrpnMessageHeader.Parse(buffer);

            // if we are using udp, the data is already in the buffer, else we have to read them
            byte[] payload;
            if (udp)
            {
                payload = new byte[header.Length];
                buffer.Skip((int) VrpnMessageHeader.HeaderLength).Take((int) header.Length).ToArray().CopyTo(payload, 0);
            }
            else
            {
                payload = new byte[header.CeiledLength];
                s.Receive(payload);
            }

            return new VrpnMessage(header, payload);
        }

        /// <summary>
        ///     Create an UDP description message to inform the server over which UDP socket data can be received.
        /// </summary>
        /// <param name="host">Local Bind Address where UDP socket listens.</param>
        /// <param name="port">Local Bind Port where UDP socket listens.</param>
        public static VrpnMessage CreateUdpDescriptionMessage(string host, int port)
        {
            var payload = Encoding.UTF8.GetBytes(host);
            var header = VrpnMessageHeader.Create(payload, port, (int) VrpnMessageType.UdpDescription);

            return new VrpnMessage(header, payload);
        }

        /// <summary>
        ///     Create a message with information about message types sent by this application.
        /// </summary>
        /// <param name="id">The id of the message.</param>
        /// <param name="sender">The type id which will be used in further messages as sender.</param>
        public static VrpnMessage CreateTypeDescriptionMessage(string id, int sender)
        {
            var data = Encoding.UTF8.GetBytes(id);
            var datalen = NetUtils.htonl((uint) data.Length);
            var payload = new byte[sizeof (uint) + data.Length];

            for (var i = 0; i < sizeof (uint); i++) payload[i] = datalen[i];
            for (var i = 0; i < data.Length; i++) payload[sizeof (uint) + i] = data[i];

            var header = VrpnMessageHeader.Create(payload, sender, (int) VrpnMessageType.TypeDescription);
            return new VrpnMessage(header, payload);
        }

        /// <summary>
        ///     Create a message with information about message senders sent by this application.
        /// </summary>
        /// <param name="id">The id of the message.</param>
        /// <param name="sender">The sender id which will be used in further messages as sender.</param>
        public static VrpnMessage CreateSenderDescriptionMessage(string id, int sender)
        {
            var data = Encoding.UTF8.GetBytes(id);
            var datalen = NetUtils.htonl((uint) data.Length);
            var payload = new byte[sizeof (uint) + data.Length];

            for (var i = 0; i < sizeof (uint); i++) payload[i] = datalen[i];
            for (var i = 0; i < data.Length; i++) payload[sizeof (uint) + i] = data[i];

            var header = VrpnMessageHeader.Create(payload, sender, (int) VrpnMessageType.SenderDescription);
            return new VrpnMessage(header, payload);
        }
    }
}