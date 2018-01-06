using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using Game.Shared.Network;
using Game.Shared.NetworkComponents.PlayerComponent;
using Game.Shared.Utility;
using Lidgren.Network;
using Nez.Console;

namespace Game.Shared.NetworkComponents.Chat
{
    public class Chat : NetworkComponent
    {
        public delegate void ChatMessageDelegate(ChatMessage message);

        public enum Channel : ushort
        {
            Public = 1,
            Vicinity = 2,
            Whisper = 3,

            //Reserve to 255 in case more special channels are needed.
            Private = 255 //255+ is a private channel.
        }

        private readonly FixedSizedQueue<ChatMessage> messageQueue = new FixedSizedQueue<ChatMessage>(255);
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public Chat()
        {
            Network.AddIncomingEventHandler(NetworkSingleton.PeerType.Client, NetworkEvents.Event.ChatMessage,
                ReceiveMessage);
            Network.AddIncomingEventHandler(NetworkSingleton.PeerType.Server, NetworkEvents.Event.ChatMessage,
                ReceiveMessage);
        }

        public IEnumerator<ChatMessage> Messages => messageQueue.GetEnumerator();

        public event ChatMessageDelegate ClientOnMessageReceived;

        /// <summary>
        ///     Sends a chat message to the server.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(ChatMessage message)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (message.Channel.Value)
            {
                case (ushort) Channel.Public:
                    //Send to all
                    Network.SendEventToServer(NetworkEvents.Event.ChatMessage, message);
                    break;
                case (ushort) Channel.Vicinity:
                    //TODO: Get position from sender then send
                    break;
            }
        }

        /// <summary>
        ///     Called when a chat message is received both on the client and server.
        /// </summary>
        /// <param name="receiverPeerType"></param>
        /// <param name="message"></param>
        private void ReceiveMessage(NetworkSingleton.PeerType receiverPeerType, NetIncomingMessage message)
        {
            var msg = message.ReadDataBytes().DeserializeEventData<ChatMessage>();
            messageQueue.Enqueue(msg);

            //Client receive message.
            if (receiverPeerType == NetworkSingleton.PeerType.Client)
            {
                DebugConsole.Instance.Log(msg);
                ClientOnMessageReceived?.Invoke(msg);
            }


            //Server received message
            if (receiverPeerType != NetworkSingleton.PeerType.Server) return;

            DebugConsole.Instance.Log(msg);

            if (msg.Channel.Value >= (ushort) Channel.Private) //Private channel
            {
                msg.Channel.Decompress(); 
                
                //TODO: get players of these two id's and send.
                return;
            }

            switch (msg.Channel.Value)
            {
                case (ushort) Channel.Public:
                    //Send to all
                    Network.SendEventToAllClients(NetworkEvents.Event.ChatMessage, message);
                    break;
                case (ushort) Channel.Vicinity:
                    //TODO: Get clients in vicinity then send.
                    break;
                case (ushort) Channel.Whisper:
                    //TODO:Get clients in whisper distance and send.
                    break;
                default:
                    DebugConsole.Instance.Log("Chat channel not supported: " + msg.Channel);
                    break;
            }
        }

        //Computes and sends a private ChatMessage.
        public void SendPrivateMessage(ChatMessage message, Player p)
        {
            var channel = new ChatMessage.ChatChannel
            {
                PlayerId1 = message.Sender,
                PlayerId2 = p.Id
            };
            channel.Compress();
            
            message.Channel = channel;
            SendMessage(message);
        }

        public override string ToString()
        {
            stringBuilder.Clear();
            foreach (var chatMessage in messageQueue)
                stringBuilder.AppendLine(chatMessage.ToString());
            return stringBuilder.ToString();
        }
    }
}