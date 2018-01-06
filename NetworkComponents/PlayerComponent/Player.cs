using System.Net.Mime;
using Game.Shared.Components.Map;
using Game.Shared.Utility;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Console;
using Nez.Sprites;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class Player : Component, IUpdatable
    {
        private Vector2 velocity = Vector2.Zero;
        public bool IsBehindObject;
        public Mover Mover;
        public Sprite Sprite;
        public bool IsRemote = true;

        public const float PlayerLayerDepth = 0.7f;

        public Player(Mover mover, Sprite sprite)
        {
            Mover = mover;
            Sprite = sprite;
        }
        
        public Player()
        {
        }

        public byte Id { get; set; }
        public NetConnection Connection { get; set; }

        public void Update()
        {
            if(!IsRemote)
                LocalInput();
            
            SortRenderDepth();
        }

        public void LocalInput()
        {
            const int speed = 200;
            if (Input.IsKeyDown(Keys.D))
                velocity += Vector2.UnitX * speed * Time.DeltaTime;
            if (Input.IsKeyDown(Keys.A))
                velocity += -Vector2.UnitX * speed * Time.DeltaTime;
            if (Input.IsKeyDown(Keys.W))
                velocity += -Vector2.UnitY * speed * Time.DeltaTime;
            if (Input.IsKeyDown(Keys.S))
                velocity += Vector2.UnitY * speed * Time.DeltaTime;

          
            Mover.Move(velocity, out var result);
            velocity = Vector2.Zero;
        }

        public override void OnAddedToEntity()
        {
            // Assert.IsTrue(Sprite.Entity != null && Mover.Entity != null,
            //    "Components must be added to entity before being passed in.");`
            Sprite.SetRenderLayer(IsometricMap.Instance.ObjectRenderLayer);
            Sprite.SetLayerDepth(0);
            // Sprite.SetColor(new Color(200,50,50,100));
            var bc = new BoxCollider(32, 32);
            Entity.AddComponent(bc);
            Entity.GetComponent<BoxCollider>().SetLocalOffset(new Vector2(0, 16));
        }

        private void SortRenderDepth()
        {
            var isBehind = IsometricMap.Instance.OverlapZones.IsBehind(this, out var collisionResult);
            Sprite.SetLayerDepth(isBehind ? 0.8f : 0.2f);
            IsBehindObject = isBehind;

            if (!isBehind) return;
            var trigger = (PolygonColliderTrigger) collisionResult.Collider;
            if (trigger.Depth != null)
            {
                Sprite.SetLayerDepth(trigger.Depth.Value);
            }
        }
        
        
    }
}