using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Nez;
using Nez.Console;

namespace Game.Shared.Network
{
    public class NetworkSingleton
    {
        //Supports running as client server or both.

        #region State

        //State
        public enum PeerType
        {
            Client,
            Server,
            Both
        }

        public PeerType LocalPeerType { get; private set; }

        #endregion

        //Network fields

        #region Network

        private NetClient client;
        private NetServer server;
        private NetPeerConfiguration serverConfiguration;
        private const string AppName = "App22";

        #endregion //Net

        #region Events and Delegates

        //Delegates
        public delegate void IncomingMessageDelegate(PeerType sender, NetIncomingMessage message);

        public delegate void ConnectionDelegate(PeerType sender, NetConnection netConenction);

        public delegate void InitializedDelegate(PeerType networkPeerType);

        //Events
        private event IncomingMessageDelegate OnClientMessageReceived;

        //Called on server
        private event IncomingMessageDelegate OnServerMessageReceived;

        public event ConnectionDelegate OnClientConnected;
        public event ConnectionDelegate OnClientDisonnect;

        //Internal
        public event InitializedDelegate OnInitialized;

        #endregion

        #region Singleton Implementation

        private static NetworkSingleton instance;

        private NetworkSingleton()
        {
        }

        public static NetworkSingleton Instance
        {
            get
            {
                instance = instance ?? new NetworkSingleton();
                return instance;
            }
        }

        #endregion

        #region Event Implementation

        //Save a step when handling network events.
        /// <summary>
        ///     Add callback for a message received
        /// </summary>
        /// <param name="recipient">event Received by Client or Server?</param>
        /// <param name="e"></param>
        /// <param name="callback"></param>
        public void AddIncomingEventHandler(PeerType recipient, NetworkEvents.Event e, IncomingMessageDelegate callback)
        {
            switch (recipient)
            {
                case PeerType.Server:
                    OnClientMessageReceived += (s, message) => { message.RunEvent(e, PeerType.Client, callback); };
                    break;
                case PeerType.Client:
                    OnServerMessageReceived += (s, message) => { message.RunEvent(e, PeerType.Server, callback); };
                    break;
            }
        }


        /// <summary>
        ///     Sends an event with a network object to server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ev"></param>
        /// <param name="obj"></param>
        /// <param name="method">Unreliable by default.</param>
        public void SendEventToServer<T>(NetworkEvents.Event ev, T obj,
            NetDeliveryMethod method = NetDeliveryMethod.Unreliable)
        {
            var msg = client.CreateMessage();
            var bytes = ev.Serialize(obj);
            msg.Write(bytes);
            client.SendMessage(msg, method);
        }

        /// <summary>
        ///     Sends an event with a network object to clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientConnections"></param>
        /// <param name="ev"></param>
        /// <param name="obj"></param>
        /// <param name="method">Unreliable by default.</param>
        public void SendEventToClient<T>(IList<NetConnection> clientConnections, NetworkEvents.Event ev, T obj,
            NetDeliveryMethod method = NetDeliveryMethod.Unreliable)
        {
            var msg = server.CreateMessage();
            var bytes = ev.Serialize(obj);
            msg.Write(bytes);
            server.SendMessage(msg, clientConnections, method, 0);
        }


        /// <summary>
        ///     Sends an event with a network object to all clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ev"></param>
        /// <param name="obj"></param>
        /// <param name="method">Unreliable by default.</param>
        public void SendEventToAllClients<T>(NetworkEvents.Event ev, T obj,
            NetDeliveryMethod method = NetDeliveryMethod.Unreliable)
        {
            var msg = server.CreateMessage();
            var bytes = ev.Serialize(obj);
            msg.Write(bytes);
            server.SendToAll(msg, method);
        }

        #endregion

        //Defines how messages are routed.

        #region Strategy Implementation

        //Stores an action to be run over a specific message type. 
        private Dictionary<NetIncomingMessageType, Action<PeerType, NetIncomingMessage>> messageLoopActions;

        private void InitializeMessageLoopActions()
        {
            //Set actions linked to incoming messages.
            messageLoopActions =
                new Dictionary<NetIncomingMessageType, Action<PeerType, NetIncomingMessage>>
                {
                    {NetIncomingMessageType.Data, IncomingMessageTypeData},
                    {NetIncomingMessageType.StatusChanged, IncomingMessageTypeStatusChanged},
                    {NetIncomingMessageType.DebugMessage, IncomingMessageTypeDebugMessage}
                };
        }

        private void IncomingMessageTypeData(PeerType receiver, NetIncomingMessage message)
        {
            if (receiver == PeerType.Client)
                OnServerMessageReceived?.Invoke(receiver, message);
            else if (receiver == PeerType.Server)
                OnClientMessageReceived?.Invoke(receiver, message);
            else
                throw new ArgumentOutOfRangeException(nameof(receiver), "Invalid NetPeer class.");
        }

        private void IncomingMessageTypeStatusChanged(PeerType t, NetIncomingMessage message)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (message.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    OnClientConnected?.Invoke(t, message.SenderConnection);
                    break;
                case NetConnectionStatus.Disconnecting:
                    OnClientDisonnect?.Invoke(t, message.SenderConnection);
                    break;
            }
        }

        private static void IncomingMessageTypeDebugMessage(PeerType t, NetIncomingMessage message)
        {
            //Only received when compiled in DEBUG mode.
            var str = message.ReadString();
            str = Enum.GetName(typeof(PeerType), t) + ": " + str;
            Debug.Log(str);
            DebugConsole.Instance.Log(str);
        }

        private static void IncomingMessageTypeDefault(PeerType t, NetIncomingMessage message)
        {
            try
            {
                var msg = string.Format("Network: Unhandled Message Type: \n\t{0}\n\t{1}",
                    message.MessageType,
                    message.ReadString());
                Debug.Log(msg);
                DebugConsole.Instance.Log(msg);
            }
            catch (Exception)
            {
                /*ignored*/
            }
        }

        #endregion

        #region Network Implementation

        /// <summary>
        ///     Initializes the network classes, but doesn't establish connection.
        /// </summary>
        /// <param name="peerType">Client, Server or Both.</param>
        /// <param name="port"></param>
        public void InitNetwork(PeerType peerType, int? port = 1777)
        {
            InitializeMessageLoopActions();
            this.LocalPeerType = peerType;

            switch (this.LocalPeerType)
            {
                case PeerType.Client:
                    var npConfig = new NetPeerConfiguration(AppName);
                    client = new NetClient(npConfig);
                    break;

                case PeerType.Server:
                    serverConfiguration = new NetPeerConfiguration(AppName);
                    if (port != null) serverConfiguration.Port = (int) port;
                    server = new NetServer(serverConfiguration);
                    break;

                case PeerType.Both:
                    serverConfiguration = new NetPeerConfiguration(AppName);
                    var cConfig = new NetPeerConfiguration(AppName);
                    cConfig.Port = (int) port + 1;
                    client = new NetClient(cConfig);
                    if (port != null) serverConfiguration.Port = (int) port;
                    server = new NetServer(serverConfiguration);
                    server.Start();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(this.LocalPeerType), this.LocalPeerType, null);
            }
            OnInitialized?.Invoke(this.LocalPeerType);
        }

        /// <summary>
        ///     Connects client to specified ip and port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            var exception = new Exception("Network not initialized, call InitNetwork().");

            Action connect = () =>
            {
                if (!IPAddress.TryParse(ip, out _)) throw new Exception("Invalid IP Address.");
                client.Start();
                client.Connect(ip, port);
            };

            switch (LocalPeerType)
            {
                case PeerType.Client:
                    if (client == null) throw exception;
                    connect.Invoke();
                    break;

                case PeerType.Server:
                    throw new Exception("A Server can't connect to another Server.");

                case PeerType.Both:
                    if (client == null || server == null) throw exception;
                    connect.Invoke();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Update()
        {
            switch (LocalPeerType)
            {
                case PeerType.Client:
                    MessageLoop(client);
                    break;
                case PeerType.Server:
                    MessageLoop(server);
                    break;
                case PeerType.Both:
                    MessageLoop(client);
                    MessageLoop(server);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Receives and redirects packets received by a peer.
        /// </summary>
        /// <typeparam name="T">as NetPeer</typeparam>
        /// <param name="peer">client or server</param>
        private void MessageLoop<T>(T peer) where T : NetPeer
        {
            var t = typeof(T) == typeof(NetClient) ? PeerType.Client : PeerType.Server;
            NetIncomingMessage message;
            while ((message = peer?.ReadMessage()) != null)
            {
                var doDefault = true;
                foreach (var action in messageLoopActions)
                {
                    if (action.Key != message.MessageType) continue;
                    action.Value.Invoke(t, message);
                    doDefault = false;
                    break;
                }

                if (!doDefault) continue;
                IncomingMessageTypeDefault(t, message);
            }
        }

        #endregion

        public bool IsLocalConnection(NetConnection connection)
        {
            return connection.Peer == client || connection.Peer == server;
        }
    }
}