using System.Net.Configuration;
using System.Runtime.InteropServices;
using Game.Shared.NetworkComponents.PlayerComponent;
using ProtoBuf;

namespace Game.Shared.NetworkComponents.Chat
{
    [ProtoContract]
    public class ChatMessage
    {
        [ProtoMember(5)] public ushort ChannelData;
        [ProtoMember(6)] public string Message;
        [ProtoMember(7)] public ushort[] Position;
        [ProtoMember(8)] public byte Sender;

        private ChatChannel channel;
        public ChatChannel Channel
        {
            get => channel;
            set
            {
                channel = value;
                ChannelData = Utility.DataCompress.TwoBytesToUShort(value.PlayerId1, value.PlayerId2);
            } 
        }

        /// <summary>
        /// ChatChannel has both an internal ushort that compresses both the channel and two player Id's if needed.
        /// </summary>
        public struct ChatChannel
        {
            public byte PlayerId1;
            public byte PlayerId2;     
            public ushort Value;

            //Compresses Value in to two PlayerIds.
            public void Compress()
            {
               Value = Utility.DataCompress.TwoBytesToUShort(PlayerId1, PlayerId2);
            }
            
            public void PrivateChannel(byte pid1, byte pid2)
            {
                PlayerId1 = pid1;
                PlayerId2 = pid2;
                Value = Utility.DataCompress.TwoBytesToUShort(PlayerId1, PlayerId2);
            }

            /// <summary>
            /// Populates the compressed player Id's.
            /// </summary>
            public void Decompress()
            {
                var ids = Utility.DataCompress.UShortToTwoBytes(Value);
                PlayerId1 = ids[0];
                PlayerId2 = ids[1];
            }
        }

        public ChatMessage(Player sender, string message, Chat.Channel channel)
        {
            Sender = sender.Id;
            Message = message;
            ChannelData = (ushort) channel;
            this.channel.Value = (ushort)channel;

            //TODO: IF vicinity based message, Position = sender.pos; //per tile pos, not per pixel.
        }

        public ChatMessage()
        {
            
        }

        public override string ToString()
        {
            return $"chat:{ChannelData}:{Sender}:{Message}";
        }
    }
}