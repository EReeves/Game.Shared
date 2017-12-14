using Game.Shared.Components.Map;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class Player : Component, IUpdatable
    {
        public bool InFront;
        public Mover Mover;
        public Sprite Sprite;
        private Vector2 velocity = Vector2.Zero;

        public Player(Mover mover, Sprite sprite)
        {
            Mover = mover;
            Sprite = sprite;
        }

        public byte Id { get; set; }
        public NetConnection Connection { get; set; }

        public void update()
        {
            var speed = 100;
            if (Input.isKeyDown(Keys.D))
                velocity += Vector2.UnitX * speed * Time.deltaTime;
            if (Input.isKeyDown(Keys.A))
                velocity += -Vector2.UnitX * speed * Time.deltaTime;
            if (Input.isKeyDown(Keys.W))
                velocity += -Vector2.UnitY * speed * Time.deltaTime;
            if (Input.isKeyDown(Keys.S))
                velocity += Vector2.UnitY * speed * Time.deltaTime;

            Mover.move(velocity, out var result);
            velocity = Vector2.Zero;
            SortRenderDepth();
        }

        public override void onAddedToEntity()
        {
            Assert.isTrue(Sprite.entity != null && Mover.entity != null,
                "Components must be added to entity before being passed in.");
            Sprite.setRenderLayer(IsometricMap.Instance.ObjectRenderLayer);
            Sprite.setLayerDepth(0);
            var bc = new BoxCollider(32, 32);
            Mover.addComponent(bc);
            Mover.getComponent<BoxCollider>().setLocalOffset(new Vector2(0, 16));
        }

        private void SortRenderDepth()
        {
            var inFront = IsometricMap.Instance.OverlapZones.IsBehind(this);
            Sprite.setLayerDepth(inFront ? 1 : 0);
            InFront = inFront;
        }
    }
}