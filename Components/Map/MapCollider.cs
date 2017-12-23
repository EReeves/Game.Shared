using Game.Shared.Components.Map;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Shared.Components
{
    internal class MapCollider
    {
        public readonly PolygonCollider[] Colliders;

        public MapCollider(IsometricMap map)
        {
            var collidersGroup = map.ObjectGroups["Colliders"];
            Colliders = new PolygonCollider[collidersGroup.Count];

            for (var i = 0; i < collidersGroup.Count; i++)
            {
                var obj = collidersGroup[i];

                var points = new Vector2[4];
                points[0] = new Vector2(obj.X, obj.Y); //Topleft
                points[1] = new Vector2(obj.X + obj.Width, obj.Y); //Topright
                points[2] = new Vector2(obj.X + obj.Width, obj.Y + obj.Height); //BotRight
                points[3] = new Vector2(obj.X, obj.Y + obj.Height); //BotLeft

                for (var o = 0; o < 4; o++) points[o] = Isometric.WorldToIsometricWorld(points[o], map);

                Colliders[i] = new PolygonCollider(points);
              
                Colliders[i].SetShouldColliderScaleAndRotateWithTransform(true);
                //entity.addComponent<BoxCollider>(colliders[i]);
            }
        }
    }
}