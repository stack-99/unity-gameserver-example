using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public class Net_LoginAccount : NetMsg
    {
        public Net_LoginAccount()
        {
            OP = (byte)NetOP.LoginAccount;
        }

        public LoginAccount LoginAccount {get;set;}
    }
}
