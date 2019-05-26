using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public abstract class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    [System.Serializable]
    public class LoginAccount : Account
    {

    }

    [System.Serializable]
    public class RegistrationAccount : Account
    {
        public string ConfirmPassword { get; set; }
    }
}
