#define DESKTOP
using System;
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

        private Chat networkChat;
        private Player p;
        private Sprite s;

        private PlayerFactory players;
        private Player localPlayer;

        private readonly UIContainer uiContainer = new UIContainer();

        private Skin uiSkin;

        public MasterScene(Skin skin = null)
        {
            uiSkin = skin;
        }

        public Action LoadAction { get; set; }

        public override void Initialize()
        {
            ClearColor = Color.CornflowerBlue;
            var renderer = new RenderLayerExcludeRenderer(1, UIComponent.UIRenderLayer);
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

       var txt2D = Content.Load<Texture2D>("player");
            players = new PlayerFactory(this, 10, txt2D);
   

            colliderEntity = CreateEntity("collider");
            var collider = new MapCollider(map);
            foreach (var t in collider.Colliders)
                colliderEntity.AddComponent(t);

            var spawnObject = map.ObjectGroups["Objects"].GetTiledObject("spawn");
            Camera.SetPosition(spawnObject.WorldPosition);
            //camera.transform.setPosition(spawn.x+camera.bounds.width/4, spawn.y-camera.bounds.height/1.5f);

            UISetUp();
            
            

        }

        private void OnClientConnected(NetworkSingleton.PeerType sender, NetConnection netconenction)
        {
            var player = players.CreatePlayer(sender, netconenction);
            
            if (player == null) return;
            //Set player position if local
            if (!player.IsRemote)
            {
                var spawnObject = map.ObjectGroups["Objects"].GetTiledObject("spawn");
                var pos = new Vector2
                {
                    X = spawnObject.WorldPosition.X,
                    Y = spawnObject.WorldPosition.Y
                };
                player.Entity.SetPosition(pos);
            }
        }

        public void UISetUp()
        {
            uiContainer.Component = new T();
            uiContainer.Entity = CreateEntity("UIEntity");
            uiContainer.Entity.AddComponent(uiContainer.Component);
        }

        private void OnNetworkInitialized(NetworkSingleton.PeerType networkPeerType)
        {
            networkChat = new Chat();
            var chatUI = uiContainer.Component.GetSubUI<IChatUI>();

            networkChat.ClientOnMessageReceived += message => { chatUI.SetChatText(networkChat.ToString()); };

            chatUI.OnChatSubmitted += (sender, message) =>
            {
                networkChat.SendMessage(new ChatMessage(players.ClientPlayer, message, Chat.Channel.Public));
            };
        }

        public override void Update()
        {
            Debugmove();
            base.Update();
        }

        private void Debugmove()
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
        }
    }
}