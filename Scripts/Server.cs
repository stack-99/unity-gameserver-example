using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Assets.Scripts.Shared;
using Assets.Scripts.ServerModels;
using Shared;
using System;
using Assets.Version_Control.Scripts.Shared.Networking;
using Assets.Scripts.Shared.Networking;
using Assets.Version_Control.Scripts.Shared;

public class Server : MonoBehaviour
{
    private Dictionary<string, ConnectionChannel> connectionChannels = new Dictionary<string, ConnectionChannel>();
    private Dictionary<int, ConnectionUser> ConnectionUsers = new Dictionary<int, ConnectionUser>();
    private Dictionary<string, User> Users = new Dictionary<string, User>();

    private int HostId;
    private int WebHostId;
    private bool IsServerStarted = false;
    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

    #region MonoBehaviour Might be deprecated
    [System.Obsolete]
    private void Start()
    {
        sw.Start();
        // Should be on client so the client object never dies between scenes
        DontDestroyOnLoad(gameObject);
        Init();
    }

    [System.Obsolete]
    private void Update()
    {
        UpdateMessagePump();

        if (sw.Elapsed.Minutes == 2)
        {
            // Send all clients that are in main menu do logic..
            foreach (var connUsers in ConnectionUsers)
            {
                SendClient(HostId, connUsers.Key, new Net_WelcomeMessageResponse() { Message = "Welcome to Genesis :D" });
            }

            sw.Reset();
        }
    }
    #endregion

    [System.Obsolete]
    public void Init()
    {
        NetworkTransport.Init();
        Debug.Log("Started the server");

        ConnectionConfig connConfig = new ConnectionConfig();
        byte connChannelId = connConfig.AddChannel(QosType.Reliable);
        byte messageChannelId = connConfig.AddChannel(QosType.Reliable);
        // UDP Huge packets
        byte ingameChannelId = connConfig.AddChannel(QosType.UnreliableFragmented);
        connectionChannels.Add("ReliableChannel", new ConnectionChannel(connChannelId, QosType.Reliable, "Handles small TCP packets"));
        connectionChannels.Add("MessageChannel",new ConnectionChannel(messageChannelId, QosType.Reliable, "Used for messages"));
        connectionChannels.Add("IngameChannel", new ConnectionChannel(ingameChannelId, QosType.UnreliableFragmented, "Channel used for huge packets ingame"));

        // Topology has to be the same on the server and client as they would be using a different road so same blueprint same road
        HostTopology hostTopo = new HostTopology(connConfig, ServerSettings.MAX_USER);

        // Server only code
        // Hosts
        // Standalone
        HostId = NetworkTransport.AddHost(hostTopo, ServerSettings.PORT, null);

        // Web
        WebHostId = NetworkTransport.AddWebsocketHost(hostTopo, ServerSettings.WEB_PORT, null);

       
        IsServerStarted = true;
    }

    #region Update methods
    [System.Obsolete]
    public void UpdateMessagePump()
    {
        if (!IsServerStarted)
            return;

        // Host Id (standalone/web)
        // Channel Id (but we have 2 channels remember) will be 0 on connect and disconnect its part of unity

        byte[] receiveBuffer = new byte[ServerSettings.RELIABLE_BYTE_SIZE];
        NetworkEventType netType = NetworkTransport.Receive(out int requestHostId, out int userConnectionId, out int connChannelId, receiveBuffer, ServerSettings.RELIABLE_BYTE_SIZE, out int recSize
            , out byte status);
        // This happens every frame (remember in update)

        
        if (status == 0)
        {
            switch (netType)
            {
                case NetworkEventType.ConnectEvent:
                    // Thread safety?? and obv check
                    ConnectionUsers.Add(userConnectionId, new ConnectionUser() { Id = userConnectionId, HostId = requestHostId });
                    Debug.Log(string.Format("User connected {0} through host {1} ", userConnectionId, requestHostId));
                    //SendClient(requestHostId, userConnectionId, new Net_ConnectToServerResponse())
                    break;
                case NetworkEventType.DataEvent:

                    //  Utilities
                    NetMsg msg = Utilities.DeserializeData<NetMsg>(receiveBuffer);
                    OnData(userConnectionId, connChannelId, requestHostId, msg);
                    
                    break;
                case NetworkEventType.DisconnectEvent:
                    ConnectionUsers.Remove(userConnectionId);
                    Debug.Log("User disconnected: " + userConnectionId);
                    break;
                case NetworkEventType.Nothing:
                    break;
                default:
                case NetworkEventType.BroadcastEvent:
                    Debug.Log("Unexpected event network");
                    break;
            }
        }
        else
        {
            // We have to remove the user if he wanted to disconnect so ye...
            ConnectionUsers.Remove(userConnectionId);
            Debug.Log(string.Format("An error occurred {0} whilst receiving a message from: {1} ", (NetworkError)status, userConnectionId));    
        }
    }

    #endregion

    [System.Obsolete]
    private void OnData(int connId, int channelId, int hostId, NetMsg msg)
    {
        Debug.Log("Received message of type " + msg.OP);
        switch ((NetOP)msg.OP)
        {
            case NetOP.CreateAccountRequest:
                Debug.Log("Create account request");
                OnCreateAccount(connId, hostId, (Net_CreateAccountRequest)msg);
                break;

            case NetOP.LoginAccountRequest:
                OnLoginAccount(connId, hostId, (Net_LoginRequest)msg);
                break;

            case NetOP.ConnectToServer:
                OnConnectToServer(connId, hostId, (Net_ConnectToServer)msg);
                break;
            case NetOP.MessageLobbyRequest:
                SendMessageToLobby(connId, hostId, (Net_MessageToLobbyRequest)msg);
                break;

            case NetOP.None:
                break;
        }
    }

    #region OnData Events Responses
    [Obsolete]
    private void OnCreateAccount(int connId, int hostId, Net_CreateAccountRequest msg)
    {
        Debug.Log("Create account message");
        UserOperations uo = new UserOperations(Users);
        var respStatus = new AccountOperations().RegistrationFieldsValid(msg.RegisterAccount);
        // Check that user does not exist <-- imp

        if(uo.DoesUserExist(msg.RegisterAccount.Username))
        {
            SendClient(hostId, connId, new Net_CreateAccountResponse() { Status = eRegistrationStatus.UsernameAlreadyExists});
        }
        else if(uo.DoesEmailExist(msg.RegisterAccount.Email))
        {
            SendClient(hostId, connId, new Net_CreateAccountResponse() { Status = eRegistrationStatus.EmailAlreadyExists });
        }
        else if (ConnectionUsers[connId].user != null)
        {
            // This shouldn't happen
            SendClient(hostId, connId, new Net_CreateAccountResponse()
            { Status = eRegistrationStatus.Unknown }); 
        }
        else
        {
            User newUser = new User()
            {
                Id = Guid.NewGuid(),
                Username = msg.RegisterAccount.Username,
                Discriminator = "0000",
                Email = msg.RegisterAccount.Email,
                Password = msg.RegisterAccount.Password,
                DateJoined = DateTime.Now,
                IsBanned = false
            };

            ConnectionUsers[connId].user = newUser;
            // added him in memory but Im simulating db well actually the memory o-o
            Users.Add(newUser.Username, newUser);
            // if successfull
          
            SendClient(hostId, connId, new Net_CreateAccountResponse() { Status = eRegistrationStatus.Success });
        }

    }

    [Obsolete]
    private void OnLoginAccount(int connId, int hostId, Net_LoginRequest msg)
    {
        // Check that the user does exist in our db... (for now list)
        UserOperations uo = new UserOperations(Users);
        Debug.Log("OnLoginAccount");
        try
        {
            // Can use Username or the email
            if (uo.CredentialsCorrect(msg.LoginAccount.UsernameOrEmail, msg.LoginAccount.Password))
            {
                var user = uo.GetUser(msg.LoginAccount.UsernameOrEmail);
                // This wil have to change to loading idk how will long it will actually take
                user.CurrentState = ePlayerState.IngameLoaded;
                ConnectionUsers[connId].user = user;

                Debug.Log(string.Format("Logged {0} in sucessfully", msg.LoginAccount.UsernameOrEmail));
                SendClient(hostId, connId, new Net_LoginResponse() { Status = eConnectionStatus.Successful, Username = user.Username
                , Discriminator = user.Discriminator});

                // well it wouldnt be like this he would need to choose a channel.../server w.e

                List<ClientUser> clientUsers = new List<ClientUser>();
                foreach(var connUsers in ConnectionUsers)
                {
                    clientUsers.Add(new ClientUser()
                    {
                        Username = connUsers.Value.user.Username,
                        Discriminator = connUsers.Value.user.Discriminator,
                        ConnectionId = connUsers.Key,
                        Status = connUsers.Value.user.CurrentState
                    });
                }

                SendClient(hostId, connId, new Net_UpdateClientUsersLobbyResponse() { ClientUsers = clientUsers });
            }
            else
            {
                Debug.LogWarning("Incorrect password");
                SendClient(hostId, connId, new Net_LoginResponse() { Status = eConnectionStatus.IncorrectDetails });
            }
        }
        catch(Exception)
        {
            Debug.LogWarning("User does not exist!");
            SendClient(hostId, connId, new Net_LoginResponse() { Status = eConnectionStatus.DoesNotExist });
        }

    }

    [Obsolete]
    private void OnConnectToServer(int connId, int hostId, Net_ConnectToServer msg)
    {
        if(msg.Status == eConnectionStatus.Successful)
        {
            // Load items
            SendClient(hostId, connId, new Net_ConnectToServer() { Status = eConnectionStatus.Successful });
            SendClient(hostId, connId, new Net_LoadDataRequest());
        }
    }
    [System.Obsolete]
    private void SendMessageToLobby(int connId, int hostId, Net_MessageToLobbyRequest msg)
    {
        Debug.Log("Send message to lobby");
        // Figure out which channel he is and send it there... but for now we only have 1 channel
        // If logged in... do a chekc for every one...
        foreach (var connUser in ConnectionUsers)
        {
            // Don't send it to the user requesting it
               // not working
            if (connUser.Key != connId)//&& connUser.Value.user.CurrentState == ePlayerState.LobbyStandBy)
            {
                Debug.Log("Sending to message client");
                SendClient(hostId, connUser.Key, new Net_MessageToLobbyResponse() { Status = eMessageStatus.Success, Text = msg.Text });
            }
        }

    }

    [System.Obsolete]
    private void SendMessageIngame(int connId, int hostId, Net_MessageToIngame msg)
    {
        Debug.Log("Send message to ingame");
        // We only have 1 room session for now
        foreach (var connUser in ConnectionUsers)
        {
            // Don't send it to the user requesting it
            if (connUser.Key != connId && connUser.Value.user.CurrentState == ePlayerState.IngameLoaded)
            {
                SendClient(hostId, connUser.Key, msg);
            }
        }

    }

    #endregion

    [System.Obsolete]
    public void SendClient(int recHostId, int connectionId, NetMsg msg)
    {
        int channelId = -1;
        switch((NetOP)msg.OP)
        {
            //case NetOP.CreateAccountRequest:
            //    channelId = connectionChannels["ReliableChannel"].ChannelId;
            //    break;

            case NetOP.CreateAccountResponse:
                channelId = connectionChannels["ReliableChannel"].ChannelId;
                break;

            case NetOP.LoginResponse:
                channelId = connectionChannels["ReliableChannel"].ChannelId;
                break;

            case NetOP.MessageIngame:
                // Not sure idk
                channelId = connectionChannels["MessageChannel"].ChannelId;
                break;

            case NetOP.MessageLobbyResponse:
                channelId = connectionChannels["MessageChannel"].ChannelId;
                break;
        }

        BufferInfo buffInfo = Utilities.SerializeData(msg);

        if (recHostId == 0 && channelId != -1)
        {
            NetworkTransport.Send(recHostId, connectionId, channelId, buffInfo.Buffer, buffInfo.UsedLength, out byte statusError);
        }
        else
        {
            // Send shutdown to client
            Debug.Log("Unknown hostid");
        }
    }


    [System.Obsolete]
    public void Shutdown()
    {
        IsServerStarted = false;
        NetworkTransport.Shutdown();
    }
}
