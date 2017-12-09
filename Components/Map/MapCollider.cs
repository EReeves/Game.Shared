using Game.Shared.Components.Map;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Shared.Components
{
    internal class MapCollider
    {
        public PolygonCollider[] colliders;
        private readonly IsometricMap map;

        public MapCollider(IsometricMap _map)
        {
            map = _map;
            var collidersGroup = map.ObjectGroups["Colliders"];
            colliders = new PolygonCollider[collidersGroup.Count];

            for (var i = 0; i < collidersGroup.Count; i++)
            {
                var obj = collidersGroup[i];

                var points = new Vector2[4];
                points[0] = new Vector2(obj.X, obj.Y); //Topleft
                points[1] = new Vector2(obj.X + obj.Width, obj.Y); //Topright
                points[2] = new Vector2(obj.X + obj.Width, obj.Y + obj.Height); //BotRight
                points[3] = new Vector2(obj.X, obj.Y + obj.Height); //BotLeft

                for (var o = 0; o < 4; o++)
                {
                    points[o] = Isometric.WorldToIsometricWorld(points[o], map);
                }

                colliders[i] = new PolygonCollider(points);
                colliders[i].setShouldColliderScaleAndRotateWithTransform(true);
                //entity.addComponent<BoxCollider>(colliders[i]);
            }
        }
    }
}