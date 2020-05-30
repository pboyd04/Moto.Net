using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public enum PacketType
    {
        XnlXCMPPacket = 0x70,
        GroupVoiceCall = 0x80,
        PrivateVoiceCall = 0x81,
        GroupDataCall = 0x83,
        PrivateDataCall = 0x84,
        /// <summary>
        /// Request to register with a master
        /// </summary>
        /// <see cref="MasterRegistrationRequest"/>
        RegistrationRequest = 0x90,
        /// <summary>
        /// Reply to register with master request
        /// </summary>
        /// <see cref="MasterRegistrationReply"/>
        RegistrationReply = 0x91,
        /// <summary>
        /// Ask for a list of peer repeaters
        /// </summary>
        /// <see cref="PeerListRequest"/>
        PeerListRequest = 0x92,
        PeerListReply  = 0x93,
        PeerRegisterRequest = 0x94,
        /// <summary>
        /// Reply to a request for a peer to register
        /// </summary>
        /// <see cref="PeerRegistrationReply"/>
        PeerRegisterReply = 0x95,
        /// <summary>
        /// Request for a master connection keep alive
        /// </summary>
        /// <see cref="MasterKeepAliveRequest"/>
        MasterKeepAliveRequest = 0x96,
        MasterKeepAliveReply = 0x97,
        PeerKeepAliveRequest = 0x98,
        PeerKeepAliveReply = 0x99,
        /// <summary>
        /// Deregister from the system
        /// </summary>
        /// <see cref="DeregistrationRequest"/>
        DeregisterRequest = 0x9A
    }
}
