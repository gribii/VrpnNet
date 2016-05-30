using System.Collections.Generic;
using System.Linq;
using VrpnNet.Core.Vrpn;

namespace VrpnNet.Core
{
    /// <summary>
    ///     Central registration authority where all local and remote types are registered.
    /// </summary>
    public class TypeRegistration
    {
        private static TypeRegistration _instance;

        private readonly Dictionary<LocalTypeKey, List<VrpnMessageHandler>> _localTypes;

        private readonly Dictionary<string, int> _localTypeIds;

        private readonly Dictionary<int, string> _remoteTypes;

        private TypeRegistration()
        {
            this._remoteTypes = new Dictionary<int, string>();
            this._localTypeIds = new Dictionary<string, int>();
            this._localTypes = new Dictionary<LocalTypeKey, List<VrpnMessageHandler>>();
        }

        /// <summary>
        ///     Singleton instance
        /// </summary>
        public static TypeRegistration Instance
            => TypeRegistration._instance ?? (TypeRegistration._instance = new TypeRegistration());

        /// <summary>
        /// Returns the id of a specific remote registered type.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <returns>Id or null</returns>
        public int? this[string name]
            => this._remoteTypes.ContainsValue(name) ? (int?)this._remoteTypes.FirstOrDefault(v => v.Value == name).Key : null;

        /// <summary>
        /// Register a local type without any handler method. Used for registering request methods.
        /// </summary>
        /// <param name="name">Message name</param>
        /// <returns></returns>
        public int RegisterLocalType(string name)
        {
            if (!this._localTypeIds.ContainsKey(name)) this._localTypeIds.Add(name, this._localTypeIds.Count == 0 ? 0 : (this._localTypeIds.Values.Max() + 1));
            return this._localTypeIds[name];
        }

        /// <summary>
        ///     Register a local type with the handler which will be used on message arrival.
        /// </summary>
        /// <param name="name">Type id</param>
        /// <param name="sender">The sender to listen for.</param>
        /// <param name="handler">Handler callback</param>
        /// <returns> the id of the registered type.</returns>
        public int RegisterLocalType(string name, string sender, VrpnMessageHandler handler)
        {
            var key = new LocalTypeKey(name, sender);
            if (!this._localTypes.ContainsKey(key)) this._localTypes.Add(key, new List<VrpnMessageHandler>());
            this._localTypes[key].Add(handler);

            return this.RegisterLocalType(name);
        }

        /// <summary>
        /// Returns the registered local type id for this type name.
        /// </summary>
        /// <param name="name">The name of the type</param>
        /// <returns>The id or null if not registered.</returns>
        public int? GetLocalTypeId(string name)
        {
            if (!this._localTypeIds.ContainsKey(name)) return null;
            return this._localTypeIds[name];
        }

        /// <summary>
        ///     Unregister a local type.
        /// </summary>
        /// <param name="name">Type id</param>
        /// <param name="sender">The sender to liste for.</param>
        /// <param name="handler">Handler callback to delete</param>
        public void UnregisterLocalType(string name, string sender, VrpnMessageHandler handler)
        {
            var key = new LocalTypeKey(name, sender);
            if (this._localTypes.ContainsKey(key)) this._localTypes[key].Remove(handler);
        }

        /// <summary>
        ///     Register a remote type from the VRPN server.
        /// </summary>
        /// <param name="sender">Sender id used for this connection.</param>
        /// <param name="name">Type id</param>
        public void RegisterRemoteType(int sender, string name)
        {
            name = name.Replace("\0", "");
            if (this[name].HasValue) this.UnregisterRemoteType(this[name].Value);
            if (this._remoteTypes.ContainsKey(sender)) this._remoteTypes[sender] = name;
            else this._remoteTypes.Add(sender, name);
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
            var sender = SenderRegistration.Instance[msg.Header.Sender];
            if (!this._remoteTypes.ContainsKey(type)) return;
            if (!this._localTypes.ContainsKey(new LocalTypeKey(this._remoteTypes[type], sender))) return;
            this._localTypes[new LocalTypeKey(this._remoteTypes[type], sender)].ForEach(handler => handler(msg));
        }

        private class LocalTypeKey
        {
            public LocalTypeKey(string name, string sender)
            {
                this.Name = name;
                this.Sender = sender;
            }

            private string Name { get; }
            private string Sender { get; }

            public override bool Equals(object obj)
            {
                var other = obj as LocalTypeKey;
                if (other == null) return false;

                return this.Name == other.Name && this.Sender == other.Sender;
            }

            protected bool Equals(LocalTypeKey other)
            {
                return string.Equals(this.Name, other.Name) && string.Equals(this.Sender, other.Sender);
            }

            public override int GetHashCode()
            {
                return ((this.Name?.GetHashCode() ?? 0)*397) ^ (this.Sender?.GetHashCode() ?? 0);
            }
        }
    }
}