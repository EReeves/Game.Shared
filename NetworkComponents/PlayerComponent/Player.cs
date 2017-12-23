using Game.Shared.Components.Map;
﻿using System;
using System.ComponentModel;
using Game.Shared.Components;
using Game.Shared.Components.Map;
using Game.Shared.Utility;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Console;
using Nez.Sprites;
using Component = Nez.Component;

namespace Game.Shared.NetworkComponents.PlayerComponent
{
    public class Player : Component, IUpdatable
    {
        public byte Id { get; set; }
        public NetConnection Connection { get; set; }
        public Mover Mover;
        public Sprite Sprite;
        private Vector2 velocity = Vector2.Zero;
        public bool InFront = false;

        public Player(Mover mover, Sprite sprite)
        {
            Mover = mover;
            Sprite = sprite;
        }

        public override void OnAddedToEntity()
        {
            Assert.IsTrue(Sprite.Entity != null && Mover.Entity != null, "Components must be added to entity before being passed in.");
            Sprite.SetRenderLayer(IsometricMap.Instance.ObjectRenderLayer);
            Sprite.SetLayerDepth(0);
            var bc = new BoxCollider(32, 32);
            Mover.AddComponent(bc);
            Mover.GetComponent<BoxCollider>().SetLocalOffset(new Vector2(0, 16));

        }
        
        private void SortRenderDepth()
        {
            var inFront = IsometricMap.Instance.OverlapZones.IsBehind(this);
           Sprite.SetLayerDepth( inFront ? 1 : 0 );
            InFront = inFront;
        }
       
        public void Update()
        {
            var speed = 100;
            if(Input.IsKeyDown(Keys.D))
                velocity += Vector2.UnitX * speed * Time.DeltaTime;
            if(Input.IsKeyDown(Keys.A))
                velocity += -Vector2.UnitX * speed * Time.DeltaTime;
            if(Input.IsKeyDown(Keys.W))
                velocity += -Vector2.UnitY * speed * Time.DeltaTime;
            if(Input.IsKeyDown(Keys.S))
                velocity += Vector2.UnitY * speed * Time.DeltaTime;

            Mover.Move(velocity, out var result);
            velocity = Vector2.Zero;
            SortRenderDepth();
        }
    }
}