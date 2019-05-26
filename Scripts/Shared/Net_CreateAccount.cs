namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public class Net_CreateAccount : NetMsg
    {
        public Net_CreateAccount()
        {
            OP = (byte)NetOP.CreateAccount;
        }

        public RegistrationAccount RegisterAccount{get;set;}

    }
}
