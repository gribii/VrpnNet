using System;
using System.Linq;

namespace VrpnNet.Core.Vrpn.Remote
{
    /// <summary>
    ///     Remote handler for supporting the button class.
    /// </summary>
    public class VrpnButtonRemote : VrpnRemoteBase
    {
        /// <summary>
        ///     Handler for &quot;Button Admin&quot; message.
        /// </summary>
        public delegate void AdminMessage(VrpnMessageHeader header, AdminData data);

        /// <summary>
        ///     Handler for &quot;Button Alert&quot; message.
        /// </summary>
        public delegate void AlertMessage(VrpnMessageHeader header, AlertData data);

        /// <summary>
        ///     Handler for &quot;Button Change&quot; message.
        /// </summary>
        public delegate void ChangeMessage(VrpnMessageHeader handler, ChangeData data);

        /// <summary>
        ///     Handler for &quot;Button States&quot; message.
        /// </summary>
        public delegate void StatesMessage(VrpnMessageHeader header, StatesData data);

        /// <summary>
        /// Event raised on new states message arrived.;
        /// </summary>
        public event StatesMessage StatesReceived;
        /// <summary>
        /// Event raised on new admin message arrived.;
        /// </summary>
        public event AdminMessage AdminReceived;
        /// <summary>
        /// Event raised on new alert message arrived.;
        /// </summary>
        public event AlertMessage AlertReceived;
        /// <summary>
        /// Event raised on new change message arrived.;
        /// </summary>
        public event ChangeMessage ChangeReceived;

        /// <summary>
        ///     Convert the raw button states data into an object.
        /// </summary>
        /// <param name="msg">Message containing headers and raw data.</param>
        private void HandleButtonStates(VrpnMessage msg)
        {
            var data = new StatesData
            {
                States = new int[BitConverter.ToInt32(msg.Payload.Take(sizeof (int)).Reverse().ToArray(), 0)]
            };

            var offset = sizeof (int);
            for (var i = 0; i < data.States.Length; i++)
            {
                data.States[i] =
                    BitConverter.ToInt32(msg.Payload.Skip(offset).Take(sizeof (int)).Reverse().ToArray(), 0);
                offset += sizeof (int);
            }

            this.StatesReceived?.Invoke(msg.Header, data);
        }

        /// <summary>
        ///     Convert the raw button admin data into an object.
        /// </summary>
        /// <param name="msg">Message containing headers and raw data.</param>
        private void HandleButtonAdmin(VrpnMessage msg)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Convert the raw button alert data into an object.
        /// </summary>
        /// <param name="msg">Message containing headers and raw data.</param>
        private void HandleButtonAlert(VrpnMessage msg)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Convert the raw button change data into an object.
        /// </summary>
        /// <param name="msg">Message containing headers and raw data.</param>
        private void HandleButtonChange(VrpnMessage msg)
        {
            var data = new ChangeData
            {
                Button = BitConverter.ToInt32(msg.Payload.Take(sizeof (int)).Reverse().ToArray(), 0),
                ButtonState =
                    BitConverter.ToInt32(msg.Payload.Skip(sizeof (int)).Take(sizeof (int)).Reverse().ToArray(), 0)
            };

            this.ChangeReceived?.Invoke(msg.Header, data);
        }

        public override void RegisterTypes()
        {
            TypeRegistration.Instance.RegisterLocalType("vrpn_Button States\0", this.HandleButtonStates);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Button Admin\0", this.HandleButtonAdmin);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Button Alert\0", this.HandleButtonAlert);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Button Change\0", this.HandleButtonChange);
        }

        /// <summary>
        ///     Parsed data for states message.
        /// </summary>
        public class StatesData
        {
            public int[] States;
        }

        /// <summary>
        ///     Parsed data for admin message.
        /// </summary>
        public class AdminData
        {
        }

        /// <summary>
        ///     Parsed data for alert message.
        /// </summary>
        public class AlertData
        {
        }

        /// <summary>
        ///     Parsed data for change message.
        /// </summary>
        public class ChangeData
        {
            public int Button;
            public int ButtonState;
        }
    }
}