using System.Collections.Generic;
using System.Linq;

namespace VrpnNet.Core
{
    /// <summary>
    ///     Central registration authority where all remote _senders are registered and can be identified.
    /// </summary>
    public class SenderRegistration
    {
        private static SenderRegistration _instance;

        private readonly Dictionary<string, int> _localSenders;

        private readonly Dictionary<int, string> _senders;

        private SenderRegistration()
        {
            this._senders = new Dictionary<int, string>();
            this._localSenders = new Dictionary<string, int>();
        }

        /// <summary>
        ///     Singleton access
        /// </summary>
        public static SenderRegistration Instance
            => SenderRegistration._instance ?? (SenderRegistration._instance = new SenderRegistration());

        /// <summary>
        ///     Return the sender name for a specific already registered sender id.
        /// </summary>
        /// <param name="id">The id of the sender.</param>
        /// <returns>The name of the sender or null.</returns>
        public string this[int id] => this._senders.ContainsKey(id) ? this._senders[id] : null;

        /// <summary>
        ///     Returns the sender id for a specific already registered sender name.
        /// </summary>
        /// <param name="name">The name of the sender</param>
        /// <returns>The id of the sender or null</returns>
        public int? this[string name]
            => this._senders.ContainsValue(name) ? (int?) this._senders.FirstOrDefault(v => v.Value == name).Key : null;

        /// <summary>
        ///     Register a new remote sender.
        /// </summary>
        public void RegisterSender(int id, string name)
        {
            name = name.Replace("\0", "");
            if (this[name].HasValue) this.UnregisterSender(this[name].Value);
            if (this._senders.ContainsKey(id)) this._senders[id] = name;
            else this._senders.Add(id, name);
        }

        /// <summary>
        ///     Unregister a remote sender.
        /// </summary>
        public void UnregisterSender(int id)
        {
            if (this._senders.ContainsKey(id)) this._senders.Remove(id);
        }

        /// <summary>
        ///     Register a local sender which can send messages to the vrpn server. If the sender is already registered just return
        ///     the id.
        /// </summary>
        /// <param name="name">The name of the sender</param>
        /// <returns></returns>
        public int RegisterLocalSender(string name)
        {
            if (!this._localSenders.ContainsKey(name))
                this._localSenders.Add(name, this._localSenders.Count == 0 ? 0 : (this._localSenders.Values.Max() + 1));
            return this._localSenders[name];
        }
    }
}