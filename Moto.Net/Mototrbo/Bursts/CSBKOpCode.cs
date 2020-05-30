using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.Bursts
{
    public enum CSBKOpCode
    {
		//These CSBK Opcodes are from TS-102 361-2 and TS-102 361-4
		VoiceServiceRequest        = 0x04,
		VoiceServiceAnswer         = 0x05,
		ChannelTiming              = 0x07,
		CALOHA                     = 0x19,
		CAHOY                      = 0x1C,
		CAckvitation               = 0x1E, //Name copied from the spec...
		RandomAccessRequest        = 0x1F,
		TSAck                      = 0x20,
		MSAck                      = 0x21, 
		Nak                        = 0x26,
		CBroadcast                 = 0x28,
		PMaint                     = 0x2A,
		PClear                     = 0x2E, 
		PProtect                   = 0x2F,
		PrivateChannelGrant        = 0x30,
		TalkgroupChannelGrant      = 0x31,
		BroadcastTGChannelGrant    = 0x32,
		SingleItemPrivateDataGrant = 0x33,
		DuplexPrivateChannelGrant  = 0x36,
		MultiItemPrivateDataGrant  = 0x37,
		OutboundActivation         = 0x38,
		MoveTSCC                   = 0x39,
		Preamble                   = 0x3D,

	    //Other CSBK Opcodes (presumably Mototrbo specific)
		MototrboRadioCheck  = 0x24
	}
}
