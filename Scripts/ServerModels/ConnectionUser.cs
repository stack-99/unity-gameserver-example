using Assets.Scripts.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ServerModels
{
    public class ConnectionUser
    {
        public int Id { get; set; }

        public int HostId { get; set; }

        public User user { get; set; }
        
    }
}
