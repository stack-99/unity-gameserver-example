using System.IO;
namespace Assets.Scripts.Shared
{
    public enum NetOP : byte
    {
        None = 0,
        CreateAccount = 1,
        LoginAccount = 2,
        ConnectToServer = 3,
        LoadData = 4,
        MessageLobby = 6,
        MessageIngame = 10,
        RegistrationResponse = 100,
        LoginResponse = 101

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
