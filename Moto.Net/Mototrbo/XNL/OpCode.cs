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
        DeviceSysMapBroadcast = 0x09,
        DataMessage = 0x0b,
        DataMessageAck = 0x0c
    }
}