using System.Collections.Generic;

namespace VrpnNet.Core
{
    /// <summary>
    ///     Central registration authority where all remote _senders are registered and can be identified.
    /// </summary>
    public class SenderRegistration
    {
        private static SenderRegistration _instance;

        private readonly List<string> _localSenders;

        private readonly Dictionary<int, string> _senders;

        private SenderRegistration()
        {
            this._senders = new Dictionary<int, string>();
            this._localSenders = new List<string>();
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
        ///     Returns if a sender is in state active listening. Client has to register to all _senders.
        /// </summary>
        /// <param name="id">The remote id of this sender.</param>
        /// <returns>true: active listening</returns>
        public bool IsActive(int id)
        {
            return this._localSenders.Contains(this[id]);
        }

        /// <summary>
        ///     Register a sender for active listening.
        /// </summary>
        public void RegisterLocalSender(string name)
        {
            if (!this._localSenders.Contains(name)) this._localSenders.Add(name);
        }

        /// <summary>
        ///     Remove a sender from active listening.
        /// </summary>
        public void UnregisterLocalSender(string name)
        {
            if (this._localSenders.Contains(name)) this._localSenders.Remove(name);
        }

        /// <summary>
        ///     Register a new remote sender.
        /// </summary>
        public void RegisterSender(int id, string name)
        {
            if (this._senders.ContainsKey(id)) this._senders[id] = name.Replace("\0", "");
            else this._senders.Add(id, name.Replace("\0", ""));
        }

        /// <summary>
        ///     Unregister a remote sender.
        /// </summary>
        public void UnregisterSender(int id)
        {
            if (this._senders.ContainsKey(id)) this._senders.Remove(id);
        }
    }
}