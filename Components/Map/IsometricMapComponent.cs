using System;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Shared.Components.Map
{
    internal class IsometricMapComponent : RenderableComponent
    {
        private readonly IsometricMap map;

        public IsometricMapComponent(IsometricMap _map)
        {
            map = _map;
        }

        public override RectangleF bounds { get => new RectangleF(); }
        public override bool isVisibleFromCamera(Camera camera)
        {
            //This is the main map class, it will always be drawn unless manually disabled.
            return true;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            for (var y = 1; y <= map.Height; y++)
            {
                for (var x = 1; x <= map.Width; x++)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach, faster, this is rendering.
                    for (var l = 0; l < map.Layers.Count; l++) //Foreach layer
                    {
                        var index = map.Layers[l].indices[x-1, y-1];//-1 because indices are zero indexed.
                        var tileset = Tileset.TilesetForPosition(index, map.Tilesets); //Tileset the index belongs to.
                        
                        var subTexturePosition = index - tileset.FirstGid; //Resets position to start at 0 for the subtexture positioning.
                        if (subTexturePosition <= -1) continue; //Don't render empty tiles.
                                                
                        var worldpos = Isometric.IsometricToWorld(new Point(x-1,y-1)); //Isometric Projection to world coords.
                        
                        //Culling 
                        if(worldpos.X > camera.bounds.x + camera.bounds.width || //Right side
                           worldpos.Y > camera.bounds.y + camera.bounds.height || //Bottom
                           worldpos.X < camera.bounds.x - map.LargestTileSize.X || //Left
                           worldpos.Y < camera.bounds.y - map.LargestTileSize.Y) //Top
                            continue; //Don't draw
                            
                            
                        var sourceRect = new Rectangle() //Subposition in texture.
                        {
                            X = subTexturePosition * tileset.TileWidth,
                            Y = (int) ((Math.Floor((subTexturePosition / (double)tileset.Columns)))*tileset.TileHeight), //Y needs rounding.
                            Width = tileset.TileWidth,
                            Height = tileset.TileHeight
                        };
                        
                        //Finally draw.
                        graphics.batcher.draw(tileset.Texture, worldpos,sourceRect,Color.White);
                    }
                }
            }
        }

        public override void debugRender(Graphics graphics)
        {
            var tl = Isometric.IsometricToWorld(new Point(1, 0));
            var tr = Isometric.IsometricToWorld(new Point(map.Width+1, 0));
            var bl = Isometric.IsometricToWorld(new Point(1, map.Height+1));
            var br = Isometric.IsometricToWorld(new Point(map.Width+1, map.Height+1));
            graphics.batcher.drawLine(tl,tr,Color.DarkOliveGreen);
            graphics.batcher.drawLine(tr,br,Color.DarkOliveGreen);
            graphics.batcher.drawLine(br,bl,Color.DarkOliveGreen);
            graphics.batcher.drawLine(bl,tl,Color.DarkOliveGreen);


            for (var y = 1; y <= map.Height; y++)
            {
                for (var x = 1; x <= map.Width; x++)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach, faster, this is rendering.
                    for (var l = 0; l < map.Layers.Count; l++) //Foreach layer
                    {
                        var index = map.Layers[l].indices[x - 1, y - 1]; //-1 because indices are zero indexed.
                        var tileset = Tileset.TilesetForPosition(index, map.Tilesets); //Tileset the index belongs to.
                        var worldpos = Isometric.IsometricToWorld(new Point(x,y)); //Isometric Projection to world coords.
                        
                        var color = new Color(150,100,100,10);
                        graphics.batcher.drawPixel(worldpos+new Vector2(0,0),color,2);
                    }
                }
            }

        }
    }
}