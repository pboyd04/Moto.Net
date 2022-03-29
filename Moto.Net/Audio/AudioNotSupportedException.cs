using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Audio
{
    public class AudioNotSupportedException : Exception
    {
        public AudioNotSupportedException(String message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
