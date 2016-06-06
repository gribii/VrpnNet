using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VrpnNet.Core.Utils;

namespace VrpnNet.Core.Vrpn
{
    /// <summary>
    ///     Represents a connection to a VRPN server. Handles connection setup and sends/receives data to/from VRPN server.
    /// </summary>
    public class VrpnConnection : IDisposable
    {
        /// <summary>
        /// Raised for every message
        /// </summary>
        public event VrpnMessageHandler MessageReceived;

        private readonly string _host;
        private readonly IPAddress _localBindAddress;

        private readonly int _port;

        private Socket _tcpControl;
        private Socket _udpData;

        /// <summary>
        ///     Initialize a new instance.
        /// </summary>
        /// <param name="host">Hostname of the VRPN server.</param>
        /// <param name="port">Port of the VRPN server.</param>
        /// <param name="localBindAddress">Address where to bind UDP/TCP sockets of this client.</param>
        public VrpnConnection(string host, int port, IPAddress localBindAddress)
        {
            this._host = host;
            this._port = port;
            this._localBindAddress = localBindAddress;
        }

        private IPAddress RemoteIp => NetUtils.Ipv4AddressFromHostname(this._host);

        internal Socket TcpControl => this._tcpControl;

        /// <summary>
        ///     Returns if a connection is established or not.
        /// </summary>
        public bool Connected
            =>
                this._tcpControl != null && this._udpData != null && this._tcpControl.Connected &&
                this._tcpControl.IsConnected();

        public void Dispose()
        {
            if (this.Connected)
            {
                this.Disconnect();
            }
        }

        /// <summary>
        ///     Initiate a control and udp data connection with a VRPN server.
        /// </summary>
        /// <param name="reconnect">How many times to try reconnecting to server. &lt;= 0 means infinite tries.</param>
        /// <param name="timeout">Time in ms how long to wait for every connect attempt.</param>
        public void Connect(int reconnect, int timeout)
        {
            if (this.Connected) throw new InvalidOperationException("Already connected");

            // create tcp and udp sockets for control channel and receiving data
            var controlListener = new TcpListener(new IPEndPoint(this._localBindAddress, 0));
            try
            {
                controlListener.Start();
                this._udpData = NetUtils.CreateUdpSocket(new IPEndPoint(this._localBindAddress, 0));

                // try setup a tcp control connection with the VRPN server.
                var attempt = 0;
                var infinite = reconnect <= 0;
                while (this._tcpControl == null && (infinite || attempt++ < reconnect))
                {
                    this.TryConnect(controlListener, timeout);
                }
                if (this._tcpControl == null) return;
            }
            finally
            {
                controlListener.Stop();
            }

            // do handshake => receive magic string from server and send our magic string
            var buffer = new byte[24];
            this._tcpControl.Receive(buffer);
            buffer = new byte[24];
            Encoding.UTF8.GetBytes(VrpnConstants.VRPN_MAGIC_HEADER).CopyTo(buffer, 0);
            this._tcpControl.Send(buffer);

            // send udp socket infos to server
            VrpnMessage.CreateUdpDescriptionMessage(this._localBindAddress.ToString(),
                (this._udpData.LocalEndPoint as IPEndPoint).Port).Send(this._tcpControl);
        }

        /// <summary>
        ///     Try initiate a connect and setup tcp control connection.
        /// </summary>
        /// <param name="controlListener">The tcp listener for the control connection.</param>
        /// <param name="timeout">Timeout in ms how long to wait until server initiates the control connection.</param>
        /// <returns>true: Successfully setup connection</returns>
        private void TryConnect(TcpListener controlListener, int timeout)
        {
            // connect to remote udp port for initiating the connection
            using (var setup = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                var msg = Encoding.UTF8.GetBytes(string.Format("{0} {1}\0",
                    this._localBindAddress, (controlListener.LocalEndpoint as IPEndPoint).Port));
                setup.Connect(new IPEndPoint(this.RemoteIp, this._port));
                setup.Send(msg);
            }

            // wait until server connects to our tcp socket
            this._tcpControl = controlListener.AcceptSocket(new TimeSpan(0, 0, 0, 0, timeout), 10);
        }

        /// <summary>
        ///     Close open tcp and udp sockets with VRPN server.
        /// </summary>
        public void Disconnect()
        {
            if (!this.Connected) throw new InvalidOperationException("Not connected");

            this._udpData.Disconnect(false);
            this._tcpControl.Disconnect(false);

            this._udpData = null;
            this._tcpControl = null;
        }

        /// <summary>
        /// Reconnect this connection.
        /// </summary>
        public void Reconnect(int retries, int timeout)
        {
            this.ForceDisconnect();
            this.Connect(retries, timeout);
        }

        /// <summary>
        ///     Force close all sockets even if not really connected.
        /// </summary>
        public void ForceDisconnect()
        {
            if (this._udpData != null && this._udpData.Connected) this._udpData?.Disconnect(false);
            if (this._tcpControl != null && this._tcpControl.Connected && this._tcpControl.IsConnected())
                this._tcpControl?.Disconnect(false);

            this._udpData = null;
            this._tcpControl = null;
        }

        /// <summary>
        ///     Read received messages from tcp control or udp connection.
        /// </summary>
        public void ReadMessages()
        {
            if (!this.Connected) return;

            // read tcp & udp messages
            this.ReadMessages(this._tcpControl);
            this.ReadMessages(this._udpData);
        }
        
        /// <summary>
        ///     Read all available messages from a socket.
        /// </summary>
        /// <param name="s"></param>
        private void ReadMessages(Socket s)
        {
            while (s.Available > 24)
            {
                var msgs = VrpnMessage.Receive(s, s.ProtocolType == ProtocolType.Udp);

#if DEBUG
                Debug.WriteLine("Received " + (s.ProtocolType) + " packet.");
#endif

                foreach (var msg in msgs)
                {
#if DEBUG
                    // Log packets
                    Debug.WriteLine("=== DATA === " + msg.ToString() + " === DATA ===");
#endif
                    switch (msg.Header.Type)
                    {
                        case (int) VrpnMessageType.SenderDescription:
                            var senderLen = BitConverter.ToUInt32(msg.Payload.Take(4).Reverse().ToArray(), 0);
                            var sender = Encoding.UTF8.GetString(msg.Payload.Skip(4).Take((int) senderLen).ToArray());
#if DEBUG
                            Debug.WriteLine(string.Format("[Sender] Sender: {0} ID: {1}", sender, msg.Header.Sender));
#endif
                            SenderRegistration.Instance.RegisterSender(msg.Header.Sender, sender);
                            break;
                        case (int) VrpnMessageType.TypeDescription:
                            var typeLen = BitConverter.ToUInt32(msg.Payload.Take(4).Reverse().ToArray(), 0);
                            var type = Encoding.UTF8.GetString(msg.Payload.Skip(4).Take((int) typeLen).ToArray());
#if DEBUG
                            Debug.WriteLine(string.Format("[Type] Type: {0} ID: {1}", type, msg.Header.Sender));
#endif
                            TypeRegistration.Instance.RegisterRemoteType(msg.Header.Sender, type);
                            break;
                        default:
#if DEBUG
                            Debug.WriteLine(string.Format("[Message] Sender: {0} Type: {1}", msg.Header.Sender,
                                msg.Header.Type));
#endif
                            TypeRegistration.Instance.ExecuteHandler(msg);
                            break;
                    }

                    // notify about received messages
                    this.MessageReceived?.Invoke(msg);
                }
            }
        }
    }
}