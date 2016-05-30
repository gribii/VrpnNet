using System.Collections.Generic;
using VrpnNet.Core.Vrpn;

namespace VrpnNet.Core
{
    /// <summary>
    ///     Central registration authority where all local and remote types are registered.
    /// </summary>
    public class TypeRegistration
    {
        private static TypeRegistration _instance;
        private readonly Dictionary<string, List<VrpnMessageHandler>> _localTypes;

        private readonly Dictionary<int, string> _remoteTypes;

        private TypeRegistration()
        {
            this._remoteTypes = new Dictionary<int, string>();
            this._localTypes = new Dictionary<string, List<VrpnMessageHandler>>();
        }

        /// <summary>
        ///     Singleton instance
        /// </summary>
        public static TypeRegistration Instance
            => TypeRegistration._instance ?? (TypeRegistration._instance = new TypeRegistration());

        /// <summary>
        ///     Register a local type with the handler which will be used on message arrival.
        /// </summary>
        /// <param name="name">Type id</param>
        /// <param name="handler">Handler callback</param>
        public void RegisterLocalType(string name, VrpnMessageHandler handler)
        {
            if (!this._localTypes.ContainsKey(name)) this._localTypes.Add(name, new List<VrpnMessageHandler>());
            this._localTypes[name].Add(handler);
        }

        /// <summary>
        ///     Unregister a local type.
        /// </summary>
        /// <param name="name">Type id</param>
        /// <param name="handler">Handler callback to delete</param>
        public void UnregisterLocalType(string name, VrpnMessageHandler handler)
        {
            if (this._localTypes.ContainsKey(name)) this._localTypes[name].Remove(handler);
        }

        /// <summary>
        ///     Register a remote type from the VRPN server.
        /// </summary>
        /// <param name="sender">Sender id used for this connection.</param>
        /// <param name="name">Type id</param>
        public void RegisterRemoteType(int sender, string name)
        {
            if (this._remoteTypes.ContainsKey(sender)) this._remoteTypes[sender] = name.Replace("\0", "");
            else this._remoteTypes.Add(sender, name.Replace("\0", ""));
        }

        /// <summary>
        ///     Unregister a remote type.
        /// </summary>
        public void UnregisterRemoteType(int sender)
        {
            this._remoteTypes.Remove(sender);
        }

        /// <summary>
        ///     Execute all message handlers for a specific message type.
        /// </summary>
        /// <param name="msg">Message received over network.</param>
        /// <remarks>The type id is stored in the message header which makes another parameter useless.</remarks>
        public void ExecuteHandler(VrpnMessage msg)
        {
            var type = msg.Header.Type;
            if (!SenderRegistration.Instance.IsActive(msg.Header.Sender)) return;
            if (!this._remoteTypes.ContainsKey(type)) return;
            if (!this._localTypes.ContainsKey(this._remoteTypes[type])) return;
            this._localTypes[this._remoteTypes[type]].ForEach(handler => handler(msg));
        }
    }
}