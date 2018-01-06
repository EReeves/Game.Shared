using System;
using System.Collections.Generic;
using Game.Shared.Network;
using Lidgren.Network;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class PlayerFactory : List<Player>
    {
        private byte id = 1;
        private readonly Texture2D defaultTexture;
        private readonly Scene scene;
        public Player ClientPlayer { get; set; }
        
        public PlayerFactory(Scene scene, int capacity, Texture2D defaultTexture = null) : base(capacity)
        {
            this.defaultTexture = defaultTexture;
            this.scene = scene;
            DefineMap();
        }

        public Player FromId(byte id)
        {
            return Find(p => p.Id == id);
        }

        /// <summary>
        /// Map enum to player blueprints.
        /// </summary>
        private Dictionary<NetworkSingleton.PeerType, Func<NetConnection,Player>> playerMap;
        private void DefineMap()
        {
            playerMap = new Dictionary<NetworkSingleton.PeerType, Func<NetConnection, Player>>
            {
                [NetworkSingleton.PeerType.Client] = connection =>
                {
                    var player = CreatePlayerBlueprint(connection);

                    //local client
                    player.IsRemote = false;
                    
                    Add(player);
                    ClientPlayer = player;
                    return player;

                },
                [NetworkSingleton.PeerType.Server] = connection =>
                {
                    //Server only needs to create remote clients.
                    if (NetworkSingleton.Instance.IsLocalConnection(connection)) return null;
                    var player = CreatePlayerBlueprint(connection);
                    player.IsRemote = true;
                    return player;
                },
                [NetworkSingleton.PeerType.Both] = CreatePlayerBlueprint
            };
        }
            
        public Player CreatePlayer(NetworkSingleton.PeerType netPeerType, NetConnection connection)
        {
            return playerMap[netPeerType].Invoke(connection);
        }

        /// <summary>
        /// Creates a basic player with entity, mover and sprite, by default player.IsRemote will be true.
        /// </summary>
        /// <returns></returns>
        private Player CreatePlayerBlueprint(NetConnection connection)
        {
            var entity = scene.CreateEntity("player" + id++);
            var mover = new Mover();
            entity.AddComponent(mover);

            var player = new Player
            {
                IsRemote = true,
                Id = id,
                Mover = mover
            };

            if (defaultTexture != null)
            {
                var sprite = new Sprite(defaultTexture);
                entity.AddComponent(sprite);
                player.Sprite = sprite;
            }
            
            entity.AddComponent(player);
            player.Connection = connection;
            
            return player;
        }
        
        
        
        
    }
}