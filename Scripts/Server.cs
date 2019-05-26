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

public class Server : MonoBehaviour
{
    private Dictionary<string, ConnectionChannel> connectionChannels = new Dictionary<string, ConnectionChannel>();
    private Dictionary<int, ConnectionUser> ConnectionUsers = new Dictionary<int, ConnectionUser>();

    private int HostId;
    private int WebHostId;
    private bool IsServerStarted = false;

    #region MonoBehaviour Might be deprecated
    [System.Obsolete]
    private void Start()
    {
        // Should be on client so the client object never dies between scenes
        DontDestroyOnLoad(gameObject);
        Init();
    }

    [System.Obsolete]
    private void Update()
    {
        UpdateMessagePump();
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
            case NetOP.CreateAccount:
                Debug.Log("Create account request");
                OnCreateAccount(connId, hostId, (Net_CreateAccount)msg);
                break;

            case NetOP.ConnectToServer:
                OnConnectToServer(connId, hostId, (Net_ConnectToServer)msg);
                break;
            case NetOP.MessageLobby:
                SendMessageToLobby(connId, hostId, (Net_MessageToLobby)msg);
                break;

            case NetOP.None:
                break;
        }
    }

    [Obsolete]
    private void OnCreateAccount(int connId, int hostId, Net_CreateAccount msg)
    {
        Debug.Log("Create account message");

        // Check that user does not exist
        // Save him so
        User newUser = new User()
        {
            Username = msg.Username,
            Email = msg.Email,
            Password = msg.Password,
            DateJoined = DateTime.Now,
            IsBanned = false
        };

        ConnectionUsers[connId].user = newUser;

        // if successfull
        SendClient(hostId, connId, new Net_RegistrationResponse() { Success = true, Message = "Successfully registered!" });
    }

 

    [Obsolete]
    private void OnConnectToServer(int connId, int hostId, Net_ConnectToServer msg)
    {
        if(msg.Status == eConnectionStatus.Successful)
        {
            // Load items
            SendClient(hostId, connId, new Net_ConnectToServer() { Status = eConnectionStatus.Successful });
            SendClient(hostId, connId, new Net_LoadData());
        }
    }
    [System.Obsolete]
    private void SendMessageToLobby(int connId, int hostId, Net_MessageToLobby msg)
    {
        Debug.Log("Send message to lobby");
        // Figure out which channel he is and send it there... but for now we only have 1 channel
        foreach (var connUser in ConnectionUsers)
        {
            // Don't send it to the user requesting it
            if (connUser.Key != connId && connUser.Value.user.CurrentState == ePlayerState.LobbyStandBy)
            {
                SendClient(hostId, connUser.Key, msg);
            }
            // we have to check anyway
            msg.Text = "Hey you i KNOW YOUUR NAME: " + ConnectionUsers[connId].user.Username;
            SendClient(hostId, connUser.Key, msg);
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

    [System.Obsolete]
    public void SendClient(int recHostId, int connectionId, NetMsg msg)
    {
        int channelId = -1;
        switch((NetOP)msg.OP)
        {
            case NetOP.CreateAccount:
                channelId = connectionChannels["ReliableChannel"].ChannelId;
                break;

            case NetOP.RegistrationResponse:
                channelId = connectionChannels["ReliableChannel"].ChannelId;
                break;

            case NetOP.MessageIngame:
                // Not sure idk
                channelId = connectionChannels["MessageChannel"].ChannelId;
                break;

            case NetOP.MessageLobby:
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
