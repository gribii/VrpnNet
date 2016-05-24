namespace VrpnNet.Core.Vrpn
{
    /// <summary>
    /// Handler for handling messages received over the network.
    /// </summary>
    /// <param name="msg">The message received over the network.</param>
    public delegate void VrpnMessageHandler(VrpnMessage msg);
}