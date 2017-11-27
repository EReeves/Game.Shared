using System.Collections.Generic;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class PlayerList : List<Player>
    {
        public PlayerList(int capacity) : base(capacity)
        {
        }

        public Player FromId(byte id)
        {
            return Find(p => p.Id == id);
        }
    }
}