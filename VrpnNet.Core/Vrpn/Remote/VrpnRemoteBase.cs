using System.CodeDom;

namespace VrpnNet.Core.Vrpn.Remote
{
    /// <summary>
    ///     Base class for remote classes.
    /// </summary>
    public abstract class VrpnRemoteBase
    {
        /// <summary>
        /// The name of the remote device.
        /// </summary>
        protected string Name { get; private set; }

        protected VrpnRemoteBase(string name)
        {
            this.Name = name;
        }

        /// <summary>
        ///     Register all types supported by this class.
        /// </summary>
        public abstract void RegisterTypes();
    }
}