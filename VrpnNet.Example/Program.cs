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
        private volatile bool _running;

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
            var analog = new VrpnAnalogRemote("DTrack");
            var button = new VrpnButtonRemote("DTrack");
            var tracker = new VrpnTrackerRemote("DTrack");
            var button2 = new VrpnButtonRemote("Mouse0");

            analog.RegisterTypes();
            button.RegisterTypes();
            tracker.RegisterTypes();
            button2.RegisterTypes();

            analog.ChannelReceived += (header, data) =>
            {
                Console.WriteLine("[Analog Remote] [{1}] {0}",
                    string.Join(",", data.Channels.Select(d => string.Format("{0:0.00}", d))),
                    SenderRegistration.Instance[header.Sender].Trim());
            };
            button.ChangeReceived += (header, data) =>
            {
                Console.WriteLine("[Button Change] [{2}] Button {0} change state to {1}",
                    data.Button, data.ButtonState,
                    SenderRegistration.Instance[header.Sender].Trim());
            };
            button2.ChangeReceived += (header, data) =>
            {
                Console.WriteLine("[Button Change] [{2}] Button {0} change state to {1}",
                    data.Button, data.ButtonState,
                    SenderRegistration.Instance[header.Sender].Trim());
            };
            button.StatesReceived += (header, data) =>
            {
                for (var i = 0; i < data.States.Length; i++)
                {
                    Console.WriteLine("[Button States] [{2}] Button {0} is in state {1}",
                        i, data.States[i],
                        SenderRegistration.Instance[header.Sender].Trim());
                }
            };
            button2.StatesReceived += (header, data) =>
            {
                for (var i = 0; i < data.States.Length; i++)
                {
                    Console.WriteLine("[Button States] [{2}] Button {0} is in state {1}",
                        i, data.States[i],
                        SenderRegistration.Instance[header.Sender].Trim());
                }
            };
            tracker.VelocityChange += (header, data) =>
            {
                Console.WriteLine("[Tracker Velocity] [{0}] Sensor {1}, Velocity [{2}], Quat [{3}], Dt {4}",
                    SenderRegistration.Instance[header.Sender].Trim(), data.Sensor,
                    string.Join(",", data.Velocity.Select(v => string.Format("{0:0.00}", v))),
                    string.Join(",", data.VelocityQuaternion.Select(v => string.Format("{0:0.00}", v))),
                    data.VelocityQuaternionDt);
            };
            tracker.PositionChange += (header, data) =>
            {
                if (data.Sensor == 2) Console.ForegroundColor = ConsoleColor.Red;
                else if (data.Sensor == 5) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Tracker Position] [{4}] [{0}] Sensor {1}, Pos [{2}], Quat [{3}]",
                    SenderRegistration.Instance[header.Sender].Trim(), data.Sensor,
                    string.Join(",", data.Position.Select(v => string.Format("{0:0.00}", v))),
                    string.Join(",", data.Orientation.Select(v => string.Format("{0:0.00}", v))),
                    header.Date.ToString("dd.MM.yyyy HH:mm:ss.fff"));
                Console.ForegroundColor = ConsoleColor.White;
            };
            tracker.TrackerToRoomChange += (header, data) =>
            {
                Console.WriteLine("[Tracker TrackerToRoom]");
            };
            tracker.UnitToSensorchange += (header, data) =>
            {
                Console.WriteLine("[Tracker UnitToSensorchange]");
            };
            tracker.WorkspaceChange += (header, data) =>
            {
                Console.WriteLine("[Tracker WorkspaceChange]");
            };
            tracker.AccelerationChange += (header, data) =>
            {
                Console.WriteLine("[Tracker Acceleration] [{0}] Sensor {1}, Acceleration [{2}], Quat [{3}], Dt {4}",
                    SenderRegistration.Instance[header.Sender].Trim(), data.Sensor,
                    string.Join(",", data.Acceleration.Select(v => string.Format("{0:0.00}", v))),
                    string.Join(",", data.AccelerationQuaternion.Select(v => string.Format("{0:0.00}", v))),
                    data.AccelerationQuaternionDt);
            };

            // connect to vrpn server
            var c = new VrpnConnection("192.168.10.50", 3883, IPAddress.Parse("192.168.10.45"));
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