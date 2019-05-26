using Assets.Scripts.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ServerModels
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Discriminator { get; set; }
        public DateTime DateJoined { get; set; }
        public bool IsBanned { get; set; }

        public bool IsAdmin { get; set; }

        public ePlayerState CurrentState { get; set; }
    }
}
