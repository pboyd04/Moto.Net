using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum Alarm
    {
        Transmit = 1,
        Receive = 2,
        Temp = 3,
        AC = 4,
        Fan = 5,
        VSWR = 20,
        TransmitPower = 23
    }

    public struct AlarmStatus
    {
        public byte Severity;
        public byte State;
        public Alarm Alarm;
    }

    public class AlarmStatusReply : XCMPPacket
    {
        protected AlarmStatus[] alarms;

        public AlarmStatusReply(byte[] data) : base(data)
        {
            if(data.Length <= 4)
            {
                this.alarms = new AlarmStatus[0];
                return;
            }
            this.alarms = new AlarmStatus[data[4]];
            for(int i = 0; i < data[4]; i++)
            {
                this.alarms[i].Severity = data[5 + (i * 7)];
                this.alarms[i].State = data[6 + (i * 7)];
                this.alarms[i].Alarm = (Alarm)data[7 + (i * 7)];
            }
        }

        public AlarmStatus[] Alarms
        {
            get
            {
                return this.alarms;
            }
        }
    }
}
