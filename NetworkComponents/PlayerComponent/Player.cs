using Lidgren.Network;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class Player
    {
        public byte Id { get; set; }
        public NetConnection Connection { get; set; }
    }
}