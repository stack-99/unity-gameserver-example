
namespace Assets.Scripts.Shared
{
    public class Net_ConnectToServer : NetMsg
    {
        public Net_ConnectToServer()
        {
            OP = (byte)NetOP.ConnectToServer;
        }

        public eConnectionStatus Status { get; set; }
    }
}
