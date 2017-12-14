#define DESKTOP
using System;
using System.Net.Http.Headers;
using System.Xml.Schema;
using Game.Shared.Components;
using Game.Shared.Components.Map;
using Game.Shared.Components.UI;
using Game.Shared.NetworkComponents.PlayerComponent;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.UI;

namespace Game.Shared.Scenes
{
    public class MasterScene<T> : Scene where T : UIComponent, new()
    {
        private Entity colliderEntity;
        private IsometricMapComponent isometricMapComponent;
        private IsometricMap map;
        private Player p;

        private Sprite s;
        private PrototypeSprite spr;
        private UIComponent uiComponent;
        private Entity uiEntity;

        private Skin uiSkin;

        public MasterScene(Skin skin = null)
        {
            uiSkin = skin;
        }

        public Action LoadAction { get; set; }

        public override void initialize()
        {
            clearColor = Color.CornflowerBlue;
            var renderer = new RenderLayerExcludeRenderer(1, UIComponent.UI_RENDER_LAYER);
            addRenderer(renderer);


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
            plyE.position = new Vector2(200, 200);

            var ef = content.loadEffect("Content/src/tint.mgfxo");
            ef.Parameters["Color"].SetValue(new Vector3(0.3f,0.3f,0.8f));          
            ef.Parameters["ColorAmount"].SetValue(1f);
   
            s.material = new Material(BlendState.AlphaBlend,ef);
            //ef.CurrentTechnique.Passes[0].Apply();


            colliderEntity = createEntity("collider");
            var collider = new MapCollider(map);
            for (var i = 0; i < collider.colliders.Length; i++) colliderEntity.addComponent(collider.colliders[i]);

            camera.setPosition(new Vector2(100, 200));
            //camera.transform.setPosition(spawn.x+camera.bounds.width/4, spawn.y-camera.bounds.height/1.5f);

            uiComponent = new T();
            uiEntity = createEntity("UIEntity");
            uiEntity.addComponent(uiComponent);
        }

        public override void update()
        {
            debugmove();
            if (spr != null)
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