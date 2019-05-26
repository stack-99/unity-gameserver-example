using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public class Net_LoginResponse : NetMsg
    {
        public Net_LoginResponse()
        {
            OP = (byte)NetOP.LoginResponse;
        }

        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
