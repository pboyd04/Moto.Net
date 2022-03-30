using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class XNLNotSupportedException : Exception
    {
        public XNLNotSupportedException(String message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
