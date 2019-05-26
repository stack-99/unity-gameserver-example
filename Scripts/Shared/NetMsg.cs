using System.IO;
namespace Assets.Scripts.Shared
{
    public enum NetOP : byte
    {
        None = 0,
        CreateAccount = 1,
        ConnectToServer = 2,
        LoadData = 3,
        MessageLobby = 6,
        MessageIngame = 10,
        RegistrationResponse = 100

    }

    [System.Serializable]
    public abstract class NetMsg
    {
        /// <summary>
        /// Operation code
        /// </summary>
        public byte OP { get; set; }

        public NetMsg()
        {
            OP = (byte)NetOP.None;
        }
    }
}
