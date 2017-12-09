using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class Player : Component, IUpdatable
    {
        public byte Id { get; set; }
        public NetConnection Connection { get; set; }
        public Mover mover;
        public Sprite Sprite;
        private Vector2 velocity = Vector2.Zero;

        public Player(Mover mover, Sprite sprite)
        {
            this.mover = mover;
        }

        public override void onAddedToEntity()
        {
            this.addComponent<Mover>(mover);
            mover.addComponent(new BoxCollider(32, 32));
            
        }

        public void update()
        {
            if(Input.isKeyDown(Keys.D))
                velocity += Vector2.UnitX;
            if(Input.isKeyDown(Keys.A))
                velocity += -Vector2.UnitX;
            if(Input.isKeyDown(Keys.W))
                velocity += -Vector2.UnitY;
            if(Input.isKeyDown(Keys.S))
                velocity += Vector2.UnitY;

            mover.move(velocity, out var result);
            if(Sprite != null)
                
            Sprite.color = result.collider != null ? Color.IndianRed : Color.White;
            velocity = Vector2.Zero;
        }
    }
}