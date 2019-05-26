using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    public class Net_MessageToIngame : NetMsg
    {
        public Net_MessageToIngame()
        {
            OP = (byte)NetOP.MessageIngame;
        }

        public string Text { get; set; }
    }
}
