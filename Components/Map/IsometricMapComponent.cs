
 using Game.Shared.Utility;
﻿using System;
using System.Globalization;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Console;

namespace Game.Shared.Components.Map
{
    internal class IsometricMapComponent : Component, IUpdatable
    {
        private readonly IsometricMap map;

        public IsometricMapComponent(IsometricMap _map)
        {
            map = _map;
        }

        public override void OnAddedToEntity()
        {
            //Add sub components.
            for (var i = 0; i < map.Layers.Count; i++)
            {
                //Set render layer before adding.
                var layer = Isometric.RENDER_LAYER_START - i;
                if (layer > map.ObjectRenderLayer && layer < map.ObjectRenderLayerEnd)
                    map.Layers[i].SetRenderLayer(layer);
                else
                    map.Layers[i].SetAsObjectPositioningLayer();

                Entity.AddComponent(map.Layers[i]);
            }
            
            map.CalculateOverlapZones(this.Entity);
        }

        public void Update()
        {

        }
    }
}