using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XCMPStatus
    {
        RSSI = 0x02,
        ModelNumber = 0x07,
        SerialNumber = 0x08,
        RepeaterSerialNumber = 0x0B,
        RadioID = 0x0E,
        RadioName = 0x0F, //Only seems to work on repeaters for some reason... very irritating...
        RadioAlias = 0x0F
    }
}

//0 - XPR5500,XPR7750e - 0
//1 - XPR5500,XPR7750e - 0
//3 - XPR5500 - error - XPR7750e - 0x26 - XPR8400 - 3
//4 - XPR5500 - error - XPR7750e - 0
//5 - XPR5500,XPR7750e - 0
//6 - XPR5500,XPR7750e - 00-00-00-00
//9 - XPR5500 - 7A-0D-67-DB-D9-78-A0-55-FF-00-FF-00-FF-00-FF-00 - XPR7750e - BB-52-45-32-38-06-15-24-09-08-07-09-09-0A-0E-FF
//10 - XPR5500,XPR7750e - FF-FF
//12 - XPR5500,XPR7750e - error
//13 - XPR5500,XPR7750e - 2
//16 - XPR5500,XPR7750e - error
//17 - XPR5500,XPR7750e - error
//18 - XPR5500,XPR7750e - error
//19 - XPR5500,XPR7750e - error
//20 - XPR5500 - 0 - XPR7750e - 5
//21 - XPR5500,XPR7750e - error
//22 - XPR5500,XPR7750e - 01-FF-FF-FF-FF-FF-FF-FF
//23 - XPR5500,XPR7750e - 1
//24 - XPR5500,XPR7750e - error
//25 - XPR5500 - causes radio to reboot!
//26 - XPR5500,XPR7750e - 01-01
//27 - XPR5500,XPR7750e - 01-01
//28 - XPR5500,XPR7750e - error
//29 - XPR5500 - 01-4B - XPR7750e - 01-53
//30 - XPR5500 - 01-5C - XPR7750e - 01-60
//31 - XPR5500,XPR7750e - error
//32 - XPR5500 - 41-52-67-77-41-51-38-55-55-6C-45-41 - ARgwAQ8UUlEA - XPR7750e - 41-54-55-51-41-52-51-6F-45-6D-49-41 - ATUQARQoEmIA
//33 - XPR5500,XPR7750e - 4
//34 - XPR5500,XPR7750e - 0
//35 - XPR5500,XPR7750e - 0
//36 - XPR5500,XPR7750e - 01-00
//37 - XPR5500,XPR7750e - FF-00
//38 - XPR5500 - 00-02 - XPR7750e - 00-01
//39 - XPR5500,XPR7750e - error
//40 - XPR5500,XPR7750e - error
//41 - XPR5500,XPR7750e - 00-40
//42 - XPR5500,XPR7750e - <empty>
//43 - XPR5500,XPR7750e - 0
//44 - XPR5500,XPR7750e - error
//45 - XPR5500,XPR7750e - error
//46 - XPR5500,XPR7750e - error
//47 - XPR5500,XPR7750e - error
//48 - XPR5500,XPR7750e - error
//49 - XPR5500,XPR7750e - error
//50 - XPR5500 - 1 - XPR7750e - 0
//51 - XPR5500,XPR7750e - error
//52 - XPR5500,XPR7750e - error
//53 - XPR5500,XPR7750e - error
//54 - XPR5500,XPR7750e - error
//55 - XPR5500,XPR7750e - error
//56 - XPR5500,XPR7750e - error
//57 - XPR5500,XPR7750e - error
//58 - 73 - XPR5500 - error
//74 - XPR5500 - hangs
//75 - XPR5500 - 16-0E-D5-4A-6D-53-D7-DB-40-90-09-C5-B3-EF-95-1B-FD-C8-FF-D7-71-13-85-5D-06-AB-76-EC-99-66-25-22 - XPR7750e - CB-E3-50-BD-CB-E2-D4-83-44-E1-AF-51-9E-FB-64-DD-16-56-03-23-3D-CD-EA-B1-F4-08-24-38-3D-E6-3B-2D
//76 - XPR5500 - 01-00 - XPR7750e - 01-02
//77 - 118 - XPR5500 - error
//119 - XPR5500 - aborts the connection
//120 - 255 - XPR5500 - error