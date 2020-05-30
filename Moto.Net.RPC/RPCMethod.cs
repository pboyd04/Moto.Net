using System;
using System.Collections.Generic;
using System.Text;

namespace Moto.Net.RPC
{
    public class RPCMethod
    {
        protected string method;
        protected object[] args;

        public RPCMethod(string method)
        {
            this.method = method;
        }

        public string Method
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        public object[] Arguments
        {
            get
            {
                return this.args;
            }
            set
            {
                this.args = value;
            }
        }
    }
}
