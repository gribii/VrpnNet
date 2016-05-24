using System.Collections.Generic;

namespace VrpnNet.Core
{
    /// <summary>
    ///     Central registration authority where all remote senders are registered and can be identified.
    /// </summary>
    public class SenderRegistration
    {
        private static SenderRegistration _instance;

        private readonly Dictionary<int, string> senders;

        private SenderRegistration()
        {
            this.senders = new Dictionary<int, string>();
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
        public string this[int id] => this.senders.ContainsKey(id) ? this.senders[id] : null;

        /// <summary>
        ///     Register a new sender.
        /// </summary>
        public void RegisterSender(int id, string name)
        {
            if (this.senders.ContainsKey(id)) this.senders[id] = name;
            else this.senders.Add(id, name);
        }

        /// <summary>
        ///     Unregister a sender.
        /// </summary>
        public void UnregisterSender(int id)
        {
            if (this.senders.ContainsKey(id)) this.senders.Remove(id);
        }
    }
}