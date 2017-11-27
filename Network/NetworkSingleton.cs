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
        public enum Type
        {
            Client,
            Server,
            Both
        }

        private Type type;

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
        public delegate void IncomingMessageDelegate(Type sender, NetIncomingMessage message);

        public delegate void ConnectionDelegate(Type sender, NetConnection netConenction);

        //Events
        private event IncomingMessageDelegate OnClientMessageReceived;

        private event IncomingMessageDelegate OnServerMessageReceived;
        public event ConnectionDelegate OnClientConnected;
        public event ConnectionDelegate OnClientDisonnect;

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
        public void AddIncomingEventHandler(Type recipient, NetworkEvents.Event e, IncomingMessageDelegate callback)
        {
            switch (recipient)
            {
                case Type.Server:
                    OnClientMessageReceived += (s, message) => { message.RunEvent(e, Type.Client, callback); };
                    break;
                case Type.Client:
                    OnServerMessageReceived += (s, message) => { message.RunEvent(e, Type.Server, callback); };
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
        private Dictionary<NetIncomingMessageType, Action<Type, NetIncomingMessage>> messageLoopActions;

        private void InitializeMessageLoopActions()
        {
            //Set actions linked to incoming messages.
            messageLoopActions =
                new Dictionary<NetIncomingMessageType, Action<Type, NetIncomingMessage>>
                {
                    {NetIncomingMessageType.Data, IncomingMessageTypeData},
                    {NetIncomingMessageType.StatusChanged, IncomingMessageTypeStatusChanged},
                    {NetIncomingMessageType.DebugMessage, IncomingMessageTypeDebugMessage}
                };
        }

        private void IncomingMessageTypeData(Type receiver, NetIncomingMessage message)
        {
            if (receiver == Type.Client)
                OnServerMessageReceived?.Invoke(receiver, message);
            else if (receiver == Type.Server)
                OnClientMessageReceived?.Invoke(receiver, message);
            else
                throw new ArgumentOutOfRangeException(nameof(receiver), "Invalid NetPeer class.");
        }

        private void IncomingMessageTypeStatusChanged(Type t, NetIncomingMessage message)
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

        private static void IncomingMessageTypeDebugMessage(Type t, NetIncomingMessage message)
        {
            //Only received when compiled in DEBUG mode.
            var str = message.ReadString();
            str = Enum.GetName(typeof(Type), t) + ": " + str;
            Debug.log(str);
            DebugConsole.instance.log(str);
        }

        private static void IncomingMessageTypeDefault(Type t, NetIncomingMessage message)
        {
            try
            {
                var msg = string.Format("Network: Unhandled Message Type: \n\t{0}\n\t{1}",
                    message.MessageType,
                    message.ReadString());
                Debug.log(msg);
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
        /// <param name="_type">Client, Server or Both.</param>
        /// <param name="port"></param>
        public void InitNetwork(Type _type, int? port = null)
        {
            InitializeMessageLoopActions();
            type = _type;

            switch (type)
            {
                case Type.Client:
                    var npConfig = new NetPeerConfiguration(AppName);
                    client = new NetClient(npConfig);
                    break;

                case Type.Server:
                    serverConfiguration = new NetPeerConfiguration(AppName);
                    if (port != null) serverConfiguration.Port = (int) port;
                    server = new NetServer(serverConfiguration);
                    break;

                case Type.Both:
                    serverConfiguration = new NetPeerConfiguration(AppName);
                    var cConfig = new NetPeerConfiguration(AppName);
                    cConfig.Port = (int) port + 1;
                    client = new NetClient(cConfig);
                    if (port != null) serverConfiguration.Port = (int) port;
                    server = new NetServer(serverConfiguration);
                    server.Start();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
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

            switch (type)
            {
                case Type.Client:
                    if (client == null) throw exception;
                    connect.Invoke();
                    break;

                case Type.Server:
                    throw new Exception("A Server can't connect to another Server.");

                case Type.Both:
                    if (client == null || server == null) throw exception;
                    connect.Invoke();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Update()
        {
            switch (type)
            {
                case Type.Client:
                    MessageLoop(client);
                    break;
                case Type.Server:
                    MessageLoop(server);
                    break;
                case Type.Both:
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
            var t = typeof(T) == typeof(NetClient) ? Type.Client : Type.Server;
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
    }
}