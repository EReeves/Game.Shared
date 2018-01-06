using System.Linq;
using Game.Shared.NetworkComponents.PlayerComponent;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Shared.Components.Map
{
    public class OverlapZones
    {
        private readonly PolygonColliderTrigger[] frontColliders;
        private readonly PolygonColliderTrigger[] backColliders;
        private readonly Entity colliderEntity;

        public OverlapZones(IsometricMap map, Entity ent)
        {
            colliderEntity = ent;
            frontColliders = LoadColliders(map, "FrontOverlapZones");
            //backColliders = LoadColliders(map, "BackOverlapZones");
        }

        private PolygonColliderTrigger[] LoadColliders(IsometricMap map, string groupName)
        {
            var collidersGroup = map.ObjectGroups[groupName];
            var colliders = new PolygonColliderTrigger[collidersGroup.Count];

            for (var i = 0; i < collidersGroup.Count; i++)
            {
                var obj = collidersGroup[i];

                var points = new Vector2[4];
                points[0] = new Vector2(obj.Position.X, obj.Position.Y); //Topleft
                points[1] = new Vector2(obj.Position.X + obj.Width, obj.Position.Y); //Topright
                points[2] = new Vector2(obj.Position.X + obj.Width, obj.Position.Y + obj.Height); //BotRight
                points[3] = new Vector2(obj.Position.X, obj.Position.Y + obj.Height); //BotLeft

                for (var o = 0; o < 4; o++) points[o] = Isometric.WorldToIsometricWorld(points[o], map);

                colliders[i] = new PolygonColliderTrigger(points);
                colliders[i].SetShouldColliderScaleAndRotateWithTransform(false);
                
                
                //Finally check to see if it has a depth property before adding
                if (obj.Properties.TryGetValue("depth", out var depth))
                {
                    float.TryParse(depth, out var floatValue);
                    colliders[i].Depth = floatValue;
                }
                
                colliderEntity.AddComponent(colliders[i]);
            }
            return colliders;
        }

        public enum Position
        {
           InFront, InMiddle, InBehind
        }
        
        public bool IsBehind(Player p, out CollisionResult collisionResult)
        {
            var collision = new CollisionResult();
            PolygonColliderTrigger collider = null;
            var result = frontColliders.Select(t =>
            {
                collider = t; //We need this collider to return in the out variable.
                return t.CollidesWith(p.GetComponent<BoxCollider>(), out collision);
            }).Any(hit => hit);

            collision.Collider = collider; //Set to the non player collider.
            collisionResult = collision; //Set out variable collisionResult.
            return result;
        }
    }
}