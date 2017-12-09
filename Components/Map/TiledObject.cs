using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Shared.Components.Map
{
    public class TiledObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    
        public Dictionary<string,string> Properties { get; } = new Dictionary<string, string>();
    }
}