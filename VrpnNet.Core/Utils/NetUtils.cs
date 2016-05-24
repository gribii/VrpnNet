using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace VrpnNet.Core.Utils
{
    /// <summary>
    /// Utilities for supporting network communication.
    /// </summary>
    public static class NetUtils
    {
        /// <summary>
        /// Returns if a socket is connected or not.
        /// </summary>
        /// <param name="s">The socket where to check connection state.</param>
        public static bool IsConnected(this Socket s)
        {
            try
            {
                return !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        /// <summary>
        /// Converts the unsigned integer from host byte order to network byte order.
        /// </summary>
        public static byte[] htonl(uint data)
        {
            return BitConverter.GetBytes(data).Reverse().Take(4).ToArray();
        }

        /// <summary>
        /// Converts the integer from host byte order to network byte order.
        /// </summary>
        public static byte[] htonl(int data)
        {
            return BitConverter.GetBytes(data).Reverse().Take(4).ToArray();
        }

        /// <summary>
        /// Create a new udp socket.
        /// </summary>
        /// <param name="endpoint">The endpoint where to bind the socket to.</param>
        /// <returns>A new socket bound on the specified endpoint or a free port.</returns>
        public static Socket CreateUdpSocket(IPEndPoint endpoint)
        {
            var socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(endpoint);
            return socket;
        }

        /// <summary>
        /// Returns the first IPv4 address for a given hostname.
        /// </summary>
        /// <param name="hostname">The hostname to find the IPv4 address for.</param>
        /// <returns>IPv4 address</returns>
        public static IPAddress Ipv4AddressFromHostname(string hostname)
        {
            return Dns.GetHostAddresses(hostname).First(a => a.AddressFamily == AddressFamily.InterNetwork);
        }

        /// <summary>
        /// Accept a socket but wait only a specified timeout.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/19653588/timeout-at-acceptsocket</remarks>
        public static Socket AcceptSocket(this TcpListener listener, TimeSpan timeout, int pollInterval)
        {
            var watch = new Stopwatch();
            watch.Start();
            while (watch.Elapsed < timeout)
            {
                if (listener.Pending()) return listener.AcceptSocket();
                Thread.Sleep(pollInterval);
            }
            return null;
        }
    }
}