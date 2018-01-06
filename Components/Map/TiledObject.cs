using System.Collections.Generic;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;

namespace Game.Shared.Components.Map
{
    public class TiledObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector2 Position = new Vector2(0,0);
        public Vector2 WorldPosition = new Vector2(0,0);
        public float Width { get; set; }
        public float Height { get; set; }

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public Vector2 ConvertCoordinatesToIsometricSpace(IsometricMap map)
        {
            WorldPosition = Isometric.WorldToIsometricWorld(Position, map);
            return WorldPosition;
        }
    }
}