using System;
using System.Diagnostics;
using System.Linq;

namespace VrpnNet.Core.Vrpn.Remote
{
    /// <summary>
    ///     Remote handler for supporting tracker class.
    /// </summary>
    public class VrpnTrackerRemote : VrpnRemoteBase
    {
        public delegate void AccelerationChangeMessage(VrpnMessageHeader header, AccelerationChangeData data);

        public delegate void PositionChangeMessage(VrpnMessageHeader header, PositionChangeData data);

        public delegate void TrackerToRoomChangeMessage(VrpnMessageHeader header, TrackerToRoomChangeData data);

        public delegate void UnitToSensorChangeMessage(VrpnMessageHeader header, UnitToSensorChangeData data);

        public delegate void VelocityChangeMessage(VrpnMessageHeader header, VelocityChangeData data);

        public delegate void WorkspaceChangeMessage(VrpnMessageHeader header, WorkspaceChangeData data);

        public event AccelerationChangeMessage AccelerationChange;
        public event PositionChangeMessage PositionChange;
        public event TrackerToRoomChangeMessage TrackerToRoomChange;
        public event UnitToSensorChangeMessage UnitToSensorchange;
        public event VelocityChangeMessage VelocityChange;
        public event WorkspaceChangeMessage WorkspaceChange;

        public VrpnTrackerRemote(string name) : base(name)
        {
        }

        private T HandleChangeBase<T>(VrpnMessage msg, bool useSensor, bool useDt) where T : ChangeDataBase, new()
        {
            var len = (9 - (useSensor ? 1 : 0) - (useDt ? 1 : 0)) *sizeof (double);
            if (msg.Header.Length != len)
            {
                Debug.WriteLine("Received invalid change message (length).");
                return null;
            }

            var data = new T {data1 = new double[3], data2 = new double[4]};
            var offset = useSensor ? 8 : 0;

            // parse sender if available
            if(!useSensor) data.sensor = null;
            else data.sensor = BitConverter.ToInt32(msg.Payload.Take(4).Reverse().ToArray(), 0);

            // parse data
            for (var i = 0; i < 3; i++)
            {
                data.data1[i] = BitConverter.ToDouble(msg.Payload.Skip(offset).Take(sizeof (double)).Reverse().ToArray(), 0);
                offset += sizeof (double);
            }
            for (var i = 0; i < 4; i++)
            {
                data.data2[i] = BitConverter.ToDouble(msg.Payload.Skip(offset).Take(sizeof (double)).Reverse().ToArray(), 0);
                offset += sizeof (double);
            }

            // parse dt value if available
            if (!useDt) data.dt = null;
            else data.dt = BitConverter.ToDouble(msg.Payload.Skip(offset).Take(sizeof (double)).Reverse().ToArray(), 0);

            return data;
        }

        private void HandleAccelerationChange(VrpnMessage msg)
        {
            var data = this.HandleChangeBase<AccelerationChangeData>(msg, true, true);
            if (data != null) this.AccelerationChange?.Invoke(msg.Header, data);
        }

        private void HandleVelocityChange(VrpnMessage msg)
        {
            var data = this.HandleChangeBase<VelocityChangeData>(msg, true, true);
            if (data != null) this.VelocityChange?.Invoke(msg.Header, data);
        }

        private void HandlePositionChange(VrpnMessage msg)
        {
            var data = this.HandleChangeBase<PositionChangeData>(msg, true, false);
            if (data != null) this.PositionChange?.Invoke(msg.Header, data);
        }

        private void HandleTrackerToRoomChange(VrpnMessage msg)
        {
            var data = this.HandleChangeBase<TrackerToRoomChangeData>(msg, false, false);
            if(data != null) this.TrackerToRoomChange?.Invoke(msg.Header, data);
        }

        private void HandleUnitToSensorChange(VrpnMessage msg)
        {
            var data = this.HandleChangeBase<UnitToSensorChangeData>(msg, true, false);
            if(data != null) this.UnitToSensorchange?.Invoke(msg.Header, data);
        }

        private void HandleWorkspaceChange(VrpnMessage msg)
        {
            if (msg.Header.Length != (6*sizeof (double)))
            {
                Debug.WriteLine("Received invalid change message (length).");
                return;
            }

            var data = new WorkspaceChangeData {MinimumCorner = new double[3], MaximumCorner = new double[3]};
            var offset = 0;
            for (var i = 0; i < 3; i++)
            {
                data.MinimumCorner[i] = BitConverter.ToDouble(msg.Payload.Skip(offset).Take(sizeof (double)).Reverse().ToArray(), 0);
                offset += sizeof (double);
            }
            for (var i = 0; i < 3; i++)
            {
                data.MaximumCorner[i] = BitConverter.ToDouble(msg.Payload.Skip(offset).Take(sizeof(double)).Reverse().ToArray(), 0);
                offset += sizeof(double);
            }

            this.WorkspaceChange?.Invoke(msg.Header, data);
        }

        public override void RegisterTypes()
        {
            TypeRegistration.Instance.RegisterLocalType("vrpn_Tracker Pos_Quat", this.Name, this.HandlePositionChange);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Tracker Acceleration", this.Name, this.HandleAccelerationChange);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Tracker Velocity", this.Name, this.HandleVelocityChange);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Tracker To_Room", this.Name, this.HandleTrackerToRoomChange);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Tracker Unit_To_Sensor", this.Name, this.HandleUnitToSensorChange);
            TypeRegistration.Instance.RegisterLocalType("vrpn_Tracker Workspace", this.Name, this.HandleWorkspaceChange);
        }

        public abstract class ChangeDataBase
        {
            internal double[] data1, data2;
            internal double? dt;
            internal int? sensor;
        }

        public class AccelerationChangeData : ChangeDataBase
        {
            public int Sensor => this.sensor.Value;
            public double[] Acceleration => this.data1;
            public double[] AccelerationQuaternion => this.data2;
            public double AccelerationQuaternionDt => this.dt.Value;
        }

        public class VelocityChangeData : ChangeDataBase
        {
            public int Sensor => this.sensor.Value;
            public double[] Velocity => this.data1;
            public double[] VelocityQuaternion => this.data2;
            public double VelocityQuaternionDt => this.dt.Value;
        }

        public class PositionChangeData : ChangeDataBase
        {
            public int Sensor => this.sensor.Value;
            public double[] Velocity => this.data1;
            public double[] Orientation => this.data2;
        }

        public class TrackerToRoomChangeData : ChangeDataBase
        {
            public double[] PositionOffset => this.data1;
            public double[] OrientationOffset => this.data2;
        }

        public class UnitToSensorChangeData : ChangeDataBase
        {
            public int Sensor => this.sensor.Value;
            public double[] PositionOffset => this.data1;
            public double[] OrientationOffset => this.data2;
        }

        public class WorkspaceChangeData
        {
            public double[] MinimumCorner;
            public double[] MaximumCorner;
        }

        public void RequestTracker2Room(VrpnConnection c)
        {
            var sender = SenderRegistration.Instance[this.Name];
            var type = TypeRegistration.Instance["vrpn_Tracker Request_Tracker_To_Room"];
            if (!sender.HasValue || !type.HasValue) return;
            var header = VrpnMessageHeader.Create(new byte[0], sender.Value, type.Value);
            var msg = new VrpnMessage(header, new byte[0]);
            msg.SendTcp(c);
        }

        public void RequestUnit2Sensor(VrpnConnection c)
        {
            var sender = SenderRegistration.Instance[this.Name];
            var type = TypeRegistration.Instance["vrpn_Tracker Request_Unit_To_Sensor"];
            if (!sender.HasValue || !type.HasValue) return;
            var header = VrpnMessageHeader.Create(new byte[0], sender.Value, type.Value);
            var msg = new VrpnMessage(header, new byte[0]);
            msg.SendTcp(c);
        }

        public void RequestWorkspace(VrpnConnection c)
        {
            var sender = SenderRegistration.Instance[this.Name];
            var type = TypeRegistration.Instance["vrpn_Tracker Request_Tracker_Workspace"];
            if (!sender.HasValue || !type.HasValue) return;
            var header = VrpnMessageHeader.Create(new byte[0], sender.Value, type.Value);
            var msg = new VrpnMessage(header, new byte[0]);
            msg.SendTcp(c);
        }
    }
}