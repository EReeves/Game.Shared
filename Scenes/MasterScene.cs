using System.Reflection;
using Game.Shared.Components;
using Game.Shared.Components.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Game.Shared.Utility;
using Nez.Analysis;
using Nez.Console;

namespace Game.Shared.Scenes
{
    public class MasterScene : Scene
    {
        private Entity colliderEntity;
        private MapCollider mapCollider;
        private IsometricMap map;
        private IsometricMapComponent isometricMapComponent;

        public override void initialize()
        {
            clearColor = Color.CornflowerBlue;
            addRenderer(new DefaultRenderer());

            var tiledEntity = createEntity("tiledmap");
            //           var map = content.Load<TmxMap>("Map/untitled");

            Benchmark.Go(() =>
            {
                var parser = new TiledParser();
                map = parser.Load(content, "/Map/untitled.tmx");
                isometricMapComponent = new IsometricMapComponent(map);
                tiledEntity.addComponent(isometricMapComponent);
            });
            
            
 
            //  var objectLayer = map.getObjectGroup("Objects");
            //  var spawn = objectLayer.objectWithName("spawn");

            colliderEntity = createEntity("collider");


            camera.setPosition(new Vector2(100, 200));
            //camera.transform.setPosition(spawn.x+camera.bounds.width/4, spawn.y-camera.bounds.height/1.5f);
        }

        public override void update()
        {
            debugmove();

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