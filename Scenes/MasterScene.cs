using System.Reflection;
using Game.Shared.Components;
using Game.Shared.Components.Map;
using Game.Shared.NetworkComponents.PlayerComponent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Game.Shared.Utility;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;

namespace Game.Shared.Scenes
{
    public class MasterScene : Scene
    {
        private Entity colliderEntity;
        private IsometricMap map;
        private IsometricMapComponent isometricMapComponent;

        private Sprite s;
        private PrototypeSprite spr;
        private Player p;

        public override void initialize()
        {
            clearColor = Color.CornflowerBlue;
            addRenderer(new DefaultRenderer());

            //           var map = content.Load<TmxMap>("Map/untitled");
            var tiledEntity = createEntity("mapEntity");
  
            Benchmark.Go(() =>
            {
                map = content.Load("/Map/untitled.tmx");
                
                isometricMapComponent = new IsometricMapComponent(map);
                tiledEntity.addComponent(isometricMapComponent);
            });
            
            
            var plyE = createEntity("plyer");
            var txt2d = content.Load<Texture2D>("player");
            s = new Sprite(txt2d);
            var mover = new Mover();
            
            plyE.addComponent(s);
            plyE.addComponent(mover);
            p = new Player(mover, s);
            plyE.addComponent(p);
            plyE.position = new Vector2(200,200);

           
            
            
  spr = new PrototypeSprite(100,100);
            
            
            
            var hello = createEntity("asd");
            spr.color = Color.Black;

            hello.addComponent(spr);
            //  var objectLayer = map.getObjectGroup("Objects");
            //  var spawn = objectLayer.objectWithName("spawn");

            colliderEntity = createEntity("collider");
            MapCollider collider = new MapCollider(map);
            for (int i = 0; i < collider.colliders.Length; i++)
            {
                colliderEntity.addComponent(collider.colliders[i]);
            }

            camera.setPosition(new Vector2(100, 200));
            //camera.transform.setPosition(spawn.x+camera.bounds.width/4, spawn.y-camera.bounds.height/1.5f);

            hello.setPosition(camera.mouseToWorldPoint());
        }

        public override void update()
        {
            debugmove();
            if(spr != null)
            spr.transform.setRotationDegrees(spr.transform.rotationDegrees + 1 % 360);
            
            base.update();
        }

        private void debugmove()
        {
            var speed = 400f * Time.deltaTime;

            if (Input.isKeyDown(Keys.Up))
                camera.setPosition(camera.position + new Vector2(0, -speed));
            if (Input.isKeyDown(Keys.Down))
                camera.setPosition(camera.position + new Vector2(0, speed));


            if (Input.isKeyDown(Keys.Left))
                camera.setPosition(camera.position + new Vector2(-speed, 0));

            if (Input.isKeyDown(Keys.Right))
                camera.setPosition(camera.position + new Vector2(speed, 0));

            base.update();
        }

    }
}