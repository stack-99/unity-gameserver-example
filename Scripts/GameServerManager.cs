using Assets.Scripts.ServerModels;
using Assets.Scripts.Shared;
using Assets.Scripts.Shared.Networking;
using Assets.Version_Control.Scripts.Shared;
using Assets.Version_Control.Scripts.Shared.Networking;
using Assets.Version_Control.Scripts.Shared.Settings_;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameServerManager : SingletonPersistent<GameServerManager>
{
    // ConnectedUsers
    public Dictionary<IClient, ClientUser> ClientUsers;
    // The saved users
    public Dictionary<string, User> Users;
    public XmlUnityServer ServerReference;
    public List<NetworkServerObject> NetworkServerObjects; 
    private bool IsServerStarted = false;
    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        sw.Start();
        ServerReference = GetComponent<XmlUnityServer>();
        ClientUsers = new Dictionary<IClient, ClientUser>();
        NetworkServerObjects = new List<NetworkServerObject>();
        Users = new Dictionary<string, User>();

        if (ServerReference != null)
        {
            ServerReference.Server.ClientManager.ClientConnected += ClientConnected;
            ServerReference.Server.ClientManager.ClientDisconnected += ClientDisconnect;
            IsServerStarted = true;
            
        }
        else
        {
            Debug.LogError("Server was not found in scene.");
        }
    }

    private void Update()
    {
        if (sw.Elapsed.Minutes == 2)
        {
            // Send all clients that are in main menu do logic..
            foreach (var connUsers in ClientUsers)
            {
                SendToClient(connUsers.Key, new Net_WelcomeMessageResponse() { Message = "Welcome to Genesis :D" }, NetworkTag.UPDATE_WELCOME_MESSAGE);
            }

            sw.Reset();
        }
    }

    #region ServerEvents
    private void ClientDisconnect(object sender, ClientDisconnectedEventArgs e)
    {
        ClientUsers.Remove(e.Client);
    }
    #endregion

    private void ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        Debug.Log("Client connected");
        e.Client.MessageReceived += Client_MessageReceived;
        ClientUsers.Add(e.Client, null);
    }

    private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            Debug.Log("Received Message!");
            
            UpdateMessagePump((NetworkTag)e.Tag, message, e.Client);
        }
    }

    private void UpdateMessagePump(NetworkTag tag, Message msg, IClient requestedClient)
    {
        if (!IsServerStarted)
        {
            Debug.Log("UpdateMessage: Not Connected to server or client did not just start.");
            return;
        }

        OnData((NetworkTag)tag, msg, requestedClient);

        //Debug.Log(string.Format("An error occurred {0} whilst receiving a message from the server: {1}. \n Was I connected?{2}" +
        //    "", (NetworkError)statusError, serverConnectionId, _IsConnectedToServer));
        // Switch to main menu and show some dialog do we shutdown?
        // Check if we are already in main menu...

    }

    private void OnData(NetworkTag tag, Message msg, IClient requestedClient)
    {
        switch (tag)
        {
            // MainMenu
            case NetworkTag.LOGIN:
                OnLoginAccount((Net_LoginRequest)msg.Deserialize<Net_LoginRequest>(), requestedClient);
                break;
            case NetworkTag.REGISTER:
                OnCreateAccount((Net_CreateAccountRequest)msg.Deserialize<Net_CreateAccountRequest>(), requestedClient);
                break;

            case NetworkTag.CONNECT_SERVER:
                OnConnectToServer((Net_ConnectToServer)msg.Deserialize<Net_ConnectToServer>(), requestedClient);
                break;

            // Lobby/Hub
            case NetworkTag.SEND_MESSAGE_LOBBY:
                SendMessageToLobby((Net_MessageToLobbyRequest)msg.Deserialize<Net_MessageToLobbyRequest>(), requestedClient.ID);
                break;
        }
        
    }

    #region OnData Events Responses
    private void OnCreateAccount(Net_CreateAccountRequest msg, IClient client)
    {
        Debug.Log("Create account message");
        UserOperations uo = new UserOperations(Users);
        var respStatus = new AccountOperations().RegistrationFieldsValid(msg.RegisterAccount);
        // Check that user does not exist <-- imp

        if (uo.DoesUserExist(msg.RegisterAccount.Username))
        {
            SendToClient(client, new Net_CreateAccountResponse() { Status = eRegistrationStatus.UsernameAlreadyExists }, NetworkTag.REGISTER);
        }
        else if (uo.DoesEmailExist(msg.RegisterAccount.Email))
        {
            SendToClient(client, new Net_CreateAccountResponse() { Status = eRegistrationStatus.EmailAlreadyExists }, NetworkTag.REGISTER);
        }
        else if (ClientUsers[client] != null)
        {
            // This shouldn't happen
            SendToClient(client, new Net_CreateAccountResponse()
            { Status = eRegistrationStatus.Unknown }, NetworkTag.REGISTER);
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
            // The connected users
            ClientUser connectedUser = new ClientUser()
            {
                Username = msg.RegisterAccount.Username,
                Discriminator = "0000",
                CurrentState = ePlayerState.LobbyStandBy
            };

            ClientUsers[client] = connectedUser;
            // added him in memory but Im simulating db well actually the memory o-o
            Users.Add(newUser.Username, newUser);
            // if successfull

            SendToClient(client, new Net_CreateAccountResponse() { Status = eRegistrationStatus.Success }, NetworkTag.REGISTER);
        }

    }

    private void OnLoginAccount(Net_LoginRequest msg, IClient client)
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
                ClientUsers[client] = new ClientUser()
                {
                    ConnectionId = client.ID,
                    Username = user.Username,
                    Discriminator = user.Discriminator,
                    CurrentState = ePlayerState.LobbyStandBy
                };

                Debug.Log(string.Format("Logged {0} in sucessfully", msg.LoginAccount.UsernameOrEmail));
                SendToClient(client, new Net_LoginResponse()
                {
                    Status = eConnectionStatus.Successful,
                    Username = user.Username
                ,
                    Discriminator = user.Discriminator
                }, NetworkTag.LOGIN);

                // well it wouldnt be like this he would need to choose a channel.../server w.e

                List<ClientUser> clientUsers = new List<ClientUser>();
                foreach (var connUsers in ClientUsers)
                {
                    clientUsers.Add(new ClientUser()
                    {
                        Username = connUsers.Value.Username,
                        Discriminator = connUsers.Value.Discriminator,
                        ConnectionId = connUsers.Key.ID,
                        CurrentState = connUsers.Value.CurrentState
                    });
                }

                SendToClient(client, new Net_UpdateClientUsersLobbyResponse() { ClientUsers = clientUsers }, NetworkTag.UPDATE_USERS_LOBBY);
            }
            else
            {
                Debug.LogWarning("Incorrect password");
                SendToClient(client, new Net_LoginResponse() { Status = eConnectionStatus.IncorrectDetails }, NetworkTag.LOGIN);
            }
        }
        catch (Exception)
        {
            Debug.LogWarning("User does not exist!");
            SendToClient(client, new Net_LoginResponse() { Status = eConnectionStatus.DoesNotExist }, NetworkTag.LOGIN);
        }

    }

    /////// todo not woring.
    private void OnConnectToServer(Net_ConnectToServer msg, IClient client)
    {
        //if (msg.Status == eConnectionStatus.Successful)
        //{
        //    // Load items
        //    SendToClient(hostId, connId, new Net_ConnectToServer() { Status = eConnectionStatus.Successful });
        //    SendClient(hostId, connId, new Net_LoadDataRequest());
        //}
    }

    #region SendMessages Responses
    private void SendMessageToLobby(Net_MessageToLobbyRequest msg, int requestedConnId)
    {
        Debug.Log("Send message to lobby");
        // Figure out which channel he is and send it there... but for now we only have 1 channel
        // If logged in... do a chekc for every one...
        foreach (var connUser in ClientUsers)
        {
            // Don't send it to the user requesting it
            // not working
            if (connUser.Key.ID != requestedConnId)//&& connUser.Value.user.CurrentState == ePlayerState.LobbyStandBy)
            {
                Debug.Log("Sending to message client");
                SendToClient(connUser.Key, new Net_MessageToLobbyResponse() { Status = eMessageStatus.Success, Text = msg.Text }, NetworkTag.SEND_MESSAGE_LOBBY);
            }
        }

    }

    private void SendMessageIngame(Net_MessageToIngame msg, IClient client)
    {
        Debug.Log("Send message to ingame");
        // We only have 1 room session for now
        foreach (var connUser in ClientUsers)
        {
            // Don't send it to the user requesting it
            if (connUser.Key.ID != client.ID && connUser.Value.CurrentState == ePlayerState.IngameLoaded)
            {
                SendToClient(connUser.Key, msg, NetworkTag.INGAME_SPAWN_OBJECT);
            }
        }

    }
    #endregion

    private bool SendToClient(IClient clientUser, NetMsg msg, NetworkTag tag)
    {
        if (clientUser != null && IsServerStarted)
        {
            Debug.Log("SendToClient.");
            using (Message message = Message.Create((ushort)tag, msg))
            {
                Debug.Log("Sending to client!");
                return clientUser.SendMessage(message, Utilities.GetChannelMode(tag));
            }
        }

        return false;
    }

    #endregion


    #region Implementation
    /// <summary>
    /// Use this function to add a network object that must be handle by the server
    /// </summary>
    /// <param name="pNetworkObject"></param>
    public void RegisterNetworkObject(NetworkServerObject pNetworkObject)
    {
        //Add the object to the list
        NetworkServerObjects.Add(pNetworkObject);
    }
    /// <summary>
    /// Send a message to the client to spawn an object into its scene
    /// </summary>
    /// <param name="pClient"></param>
    public void SendObjectToSpawnTo(NetworkServerObject pNetworkObject, IClient pClient)
    {
        //Spawn data to send
        SpawnMessageModel spawnMessageData = new SpawnMessageModel
        {
            networkID = pNetworkObject.id,
            x = pNetworkObject.gameObject.transform.position.x,
            y = pNetworkObject.gameObject.transform.position.y
        };
        //create the message 
        using (Message m = Message.Create(
            (ushort)NetworkTag.INGAME_SPAWN_OBJECT,                //Tag
            spawnMessageData)                               //Data
        )
        {
            //Send the message in TCP mode (Reliable)
            pClient.SendMessage(m, SendMode.Reliable);
        }
    }
    /// <summary>
    /// Send a message with all objects to spawn
    /// </summary>
    /// <param name="pClient"></param>
    public void SendAllObjectsToSpawnTo(IClient pClient)
    {
        foreach (NetworkServerObject networkObject in NetworkServerObjects)
            SendObjectToSpawnTo(networkObject, pClient);
    }
    #endregion

}
