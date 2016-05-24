namespace VrpnNet.Core.Vrpn
{
    /// <summary>
    /// Type of the message.
    /// </summary>
    public enum VrpnMessageType
    {
        /// <summary>
        /// Message contains sender information.
        /// </summary>
        SenderDescription = -1,
        /// <summary>
        /// Message contains type declarations.
        /// </summary>
        TypeDescription = -2,
        /// <summary>
        /// Message contains the data which UDP socket the other side can use.
        /// </summary>
        UdpDescription = -3
    }
}