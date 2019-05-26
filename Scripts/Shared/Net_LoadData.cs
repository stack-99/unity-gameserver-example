using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    public class Net_LoadData : NetMsg
    {
        public Net_LoadData()
        {
            OP = (byte)NetOP.LoadData;
        }

        public List<int> WeaponIds { get; set; }
        public List<int> ArmorIds { get; set; }
        public List<int> ItemIds { get; set; }

    }
}
