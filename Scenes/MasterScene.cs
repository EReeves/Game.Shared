#define DESKTOP
using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Xml.Schema;
using Game.Shared.Components;
using Game.Shared.Components.Map;
using Game.Shared.Components.UI;
using Game.Shared.Network;
using Game.Shared.NetworkComponents.Chat;
using Game.Shared.NetworkComponents.PlayerComponent;
using Game.Shared.Utility;
using Lidgren.Network;
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

        private Skin uiSkin;

        private Chat networkChat;

        private UIContainer _UIContainer = new UIContainer();
        
        public MasterScene(Skin skin = null)
        {
            uiSkin = skin;
        }

        public Action LoadAction { get; set; }

        public override void Initialize()
        {
            ClearColor = Color.CornflowerBlue;
            var renderer = new RenderLayerExcludeRenderer(1, UIComponent.UI_RENDER_LAYER);
            AddRenderer(renderer);


            //           var map = content.Load<TmxMap>("Map/untitled");
            var tiledEntity = CreateEntity("mapEntity");

            Benchmark.Go(() =>
            {
                map = Content.Load("/Map/untitled.tmx");

                isometricMapComponent = new IsometricMapComponent(map);
                tiledEntity.AddComponent(isometricMapComponent);
            });

            NetworkSingleton.Instance.OnInitialized += OnNetworkInitialized;
            NetworkSingleton.Instance.OnClientConnected += OnClientConnected;

            var plyE = CreateEntity("plyer");
            
            var txt2d = Content.Load<Texture2D>("player");
            var s = new Sprite(txt2d);
            var mover = new Mover();

            plyE.Position = new Vector2(200, 200);
            
            p = new Player(mover, s);
            plyE.AddComponent(s);
            plyE.AddComponent(mover);
            plyE.AddComponent(p);

            var ef = Content.LoadEffect("Content/src/tint.mgfxo");
            ef.Parameters["Color"].SetValue(new Vector3(0.3f,0.3f,0.8f));          
            ef.Parameters["ColorAmount"].SetValue(1f);
   
            s.Material = new Material(BlendState.AlphaBlend,ef);
            //ef.CurrentTechnique.Passes[0].Apply();
            plyE.Position = new Vector2(200,200);        
            
   
            colliderEntity = CreateEntity("collider");
            var collider = new MapCollider(map);
            foreach (var t in collider.Colliders)
                colliderEntity.AddComponent(t);

            Camera.SetPosition(new Vector2(100, 200));
            //camera.transform.setPosition(spawn.x+camera.bounds.width/4, spawn.y-camera.bounds.height/1.5f);

            UISetUp();
        }

        private void OnClientConnected(NetworkSingleton.Type sender, NetConnection netconenction)
        {
            var msg = new ChatMessage(p,"Hello there", Chat.Channel.Public);
            networkChat.SendMessage(msg);
        }

        public void UISetUp()
        {
            _UIContainer.Component = new T();
            _UIContainer.Entity = CreateEntity("UIEntity");
            _UIContainer.Entity.AddComponent(_UIContainer.Component);
        }

        private void OnNetworkInitialized(NetworkSingleton.Type networkType)
        {
            networkChat = new Chat();
            var chatUI = _UIContainer.Component.GetSubUI<IChatUI>();

            networkChat.ClientOnMessageReceived += message =>
            {
                chatUI.SetChatText(networkChat.ToString());
            };  
        }

        public override void Update()
        {
            debugmove();    
            base.Update();
        }

        private void debugmove()
        {
            var speed = 400f * Time.DeltaTime;

            if (Input.IsKeyDown(Keys.Up))
                Camera.SetPosition(Camera.Position + new Vector2(0, -speed));
            if (Input.IsKeyDown(Keys.Down))
                Camera.SetPosition(Camera.Position + new Vector2(0, speed));


            if (Input.IsKeyDown(Keys.Left))
                Camera.SetPosition(Camera.Position + new Vector2(-speed, 0));

            if (Input.IsKeyDown(Keys.Right))
                Camera.SetPosition(Camera.Position + new Vector2(speed, 0));

            base.Update();
        }
    }
}