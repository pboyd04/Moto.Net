namespace Moto.Net.Mototrbo.XNL
{
    public enum OpCode
    {
        MasterStatusBroadcast = 0x02,
        DeviceMasterQuery = 0x03,
        DeviceAuthKeyRequest = 0x04,
        DeviceAuthKeyReply = 0x05,
        DeviceConnectionRequest = 0x06,
        DeviceConnectionReply = 0x07,
        DeviceSysMapRequest = 0x08, //This just kicks off a new DeviceSysMapBroadcast from the connected radio. I haven't found a need for this yet.
        DeviceSysMapBroadcast = 0x09,
        DataMessage = 0x0b,
        DataMessageAck = 0x0c
    }
}