using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo
{
    public class PacketParsingException : Exception
    {
        public PacketParsingException(string message) : base(message)
        {

        }
    }
}
