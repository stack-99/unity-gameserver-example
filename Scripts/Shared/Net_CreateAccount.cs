namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public class Net_CreateAccount : NetMsg
    {
        public Net_CreateAccount()
        {
            OP = (byte)NetOP.CreateAccount;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

    }
}
