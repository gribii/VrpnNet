namespace VrpnNet.Core.Vrpn
{
    public static class VrpnConstants
    {
        /// <summary>
        ///     The maximum number of bytes sent in once when using UDP.
        /// </summary>
        public const int VRPN_UDP_BUFFER_LENGTH = 1472;

        /// <summary>
        ///     Alignment used for data. All messages will align to this value.
        /// </summary>
        public const uint VRPN_ALIGN = 8U;

        /// <summary>
        ///     The length in bytes of the header data.
        /// </summary>
        public const uint VRPN_HEADER_LENGTH = 24U;

        /// <summary>
        ///     The magic version header used for handshake.
        /// </summary>
        public const string VRPN_MAGIC_HEADER = "vrpn: ver. 07.34  0";
    }
}