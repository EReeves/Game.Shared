using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Shared.Components.Map
{
    internal class IsometricMapComponent : RenderableComponent
    {
        private IsometricMap map;

        public IsometricMapComponent(IsometricMap _map)
        {
            map = _map;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            for (var y = 0; y < map.TileHeight; y++)
            {
                for (var x = 0; x < map.TileWidth; x++)
                {
                    for (var l = 0; l < map.Layers.Count; l++)
                    {
                        var i = map.Layers[l].indices[x, y];

                        var tileset = Tileset.TilesetForPosition(i, map.Tilesets);
                        var pos = i - tileset.FirstGid;
                        
                        var sourceRect = new Rectangle()
                        {
                            X = pos*map.TileWidth,
                            Y = pos*map.TileHeight,
                            Width = map.TileWidth,
                            Height = map.TileHeight
                        };
                        graphics.batcher.draw(tileset.Texture, sourceRect);
                    }
                }
            }
        }

        public override void debugRender(Graphics graphics)
        {
            base.debugRender(graphics);
        }
    }
}