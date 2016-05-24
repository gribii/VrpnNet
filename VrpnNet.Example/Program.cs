using System;
using System.Linq;
using System.Net;
using System.Threading;
using VrpnNet.Core;
using VrpnNet.Core.Vrpn;
using VrpnNet.Core.Vrpn.Remote;

namespace VrpnNet.Example
{
    public class Program
    {
        private volatile bool _running = false;

        private void ReadMessageHandler(object connection)
        {
            var c = connection as VrpnConnection;
            if (c == null) return;

            while (this._running)
            {
                while (!c.Connected)
                {
                    Console.WriteLine("Trying to reconnect");
                    c.ForceDisconnect();
                    c.Connect(1, 1000);
                }

                c.ReadMessages();
                Thread.Sleep(1);
            }
        }

        private void Start()
        {
            // register message handlers
            var analog = new VrpnAnalogRemote();
            var button = new VrpnButtonRemote();

            analog.RegisterTypes();
            button.RegisterTypes();

            analog.ChannelReceived += (header, data) =>
            {
                Console.WriteLine("[Analog Remote] [{1}] {0}",
                    string.Join(",", data.Channels.Select(d => string.Format("{0:0.00}", d))),
                    SenderRegistration.Instance[header.Sender].Replace("\0", "").Trim());
            };
            button.ChangeReceived += (header, data) =>
            {
                Console.WriteLine("[Button Change] [{2}] Button {0} change state to {1}",
                    data.Button, data.ButtonState,
                    SenderRegistration.Instance[header.Sender].Replace("\0", "").Trim());
            };
            button.StatesReceived += (header, data) =>
            {
                for (var i = 0; i < data.States.Length; i++)
                {
                    Console.WriteLine("[Button States] [{2}] Button {0} is in state {1}",
                        i, data.States[i],
                        SenderRegistration.Instance[header.Sender].Replace("\0", "").Trim());
                }
            };

            // connect to vrpn server
            var c = new VrpnConnection("localhost", 3883, IPAddress.Parse("127.0.0.1"));
            c.Connect(3, 1000);

            // start message reader thread
            this._running = true;
            new Thread(this.ReadMessageHandler).Start(c);
        }

        private void Stop()
        {
            this._running = false;
        }


        public static void Main(string[] args)
        {
            var me = new Program();
            me.Start();

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            me.Stop();
        }
    }
}