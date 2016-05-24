namespace VrpnNet.Core.Vrpn.Remote
{
    /// <summary>
    ///     Base class for remote classes.
    /// </summary>
    public abstract class VrpnRemoteBase
    {
        /// <summary>
        ///     Register all types supported by this class.
        /// </summary>
        public abstract void RegisterTypes();
    }
}