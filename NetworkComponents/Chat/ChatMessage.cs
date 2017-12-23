using Game.Shared.NetworkComponents.PlayerComponent;
using ProtoBuf;

namespace Game.Shared.NetworkComponents.Chat
{
    [ProtoContract]
    public class ChatMessage
    {
        [ProtoMember(7)] public ushort Channel;
        [ProtoMember(5)] public string Message;
        [ProtoMember(8)] public ushort[] Position;
        [ProtoMember(6)] public byte Sender;

        public ChatMessage(Player sender, string message, Chat.Channel channel)
        {
            Sender = sender.Id;
            Message = message;
            Channel = (ushort) channel;

            //TODO: IF vicinity based message, Position = sender.pos; //per tile pos, not per pixel.
        }

        public ChatMessage()
        {
        }

        public override string ToString()
        {
            return $"chat:{Channel}:{Sender}:{Message}";
        }
    }
}