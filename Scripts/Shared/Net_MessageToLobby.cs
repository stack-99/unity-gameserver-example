namespace Assets.Scripts.Shared
{
    [System.Serializable]
    public class Net_MessageToLobby : NetMsg
    {
        public Net_MessageToLobby()
        {
            OP = (byte)NetOP.MessageLobby;
        }

        public string Text { get; set; }
    }
}
