using System.Linq;
using Game.Shared.NetworkComponents.PlayerComponent;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Game.Shared.Components.Map
{
    public class OverlapZones
    {
        private readonly PolygonColliderTrigger[] colliders;

        public OverlapZones(IsometricMap map, Entity ent)
        {
            var collidersGroup = map.ObjectGroups["FrontOverlapZones"];
            colliders = new PolygonColliderTrigger[collidersGroup.Count];

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

                colliders[i] = new PolygonColliderTrigger(points);
                colliders[i].setShouldColliderScaleAndRotateWithTransform(false);
                ent.addComponent(colliders[i]);
            }

        }

        public bool IsBehind(Player p)
        {
            return colliders.Select(t => t.collidesWith(p.getComponent<BoxCollider>(), out var _)).Any(hit => hit);
        }
    }
}