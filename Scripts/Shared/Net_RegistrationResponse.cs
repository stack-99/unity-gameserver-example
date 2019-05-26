namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public class Net_RegistrationResponse : NetMsg
    {
        public Net_RegistrationResponse()
        {
            OP = (byte)NetOP.RegistrationResponse;
        }

        public bool Success { get; set; }

        public string Message { get; set; }


    }
}
