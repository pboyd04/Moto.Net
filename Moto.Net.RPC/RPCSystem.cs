using System;
using System.Collections.Generic;
using System.Text;

namespace Moto.Net.RPC
{
    public class RPCSystem
    {
        protected RPCRadio master;

        public RPCRadio Master
        {
            get
            {
                return this.master;
            }
            set
            {
                this.master = value;
            }
        }
    }
}
