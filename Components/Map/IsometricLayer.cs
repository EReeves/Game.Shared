using System;
using System.Collections.Generic;
using Game.Shared.NetworkComponents.PlayerComponent;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Game.Shared.Components.Map
{
    public class IsometricLayer : RenderableComponent
    {
        public string Name { get; set; }
        public DenseArray<int> indices { get; }
        private readonly IsometricMap map;
        public bool IsObjectPositioningLayer = false;
        
        public const float LAYER_DEPTH_BACK = 0f;
        public const float LAYER_DEPTH_OBJBACK = 0.3f;
        public const float LAYER_DEPTH_OBJFRONT = 0.6f;
        public const float LAYER_DEPTH_FRONT = 0.9f;

        public IsometricLayer(int sizex, int sizey, bool isObjectPositioningLayer = false)
        {
            indices = new DenseArray<int>(sizey, sizex);
            map = IsometricMap.Instance;

            if (!isObjectPositioningLayer) return;
                SetAsObjectPositioningLayer();
        }

        public void SetAsObjectPositioningLayer()
        {
            IsObjectPositioningLayer = true;
            setRenderLayer(map.ObjectRenderLayer);
            setLayerDepth(LAYER_DEPTH_OBJFRONT);
        }

        public static int ObjectRenderLayerPosition(IList<IsometricLayer> list)
        {
            const string OBJECT_POSITIONING_LAYER_NAME = "ObjectPositioningLayer";
            for (var i = 0; i < list.Count; i++)
                if (list[i].Name == OBJECT_POSITIONING_LAYER_NAME)
                    return Isometric.RENDER_LAYER_START - i;
            throw new Exception($"{OBJECT_POSITIONING_LAYER_NAME} does not exist in tmx file.");
        }

        public override RectangleF bounds => new RectangleF();

        public override bool isVisibleFromCamera(Camera camera)
        {
            //This is the main map class, it will always be drawn unless manually disabled.
            return true;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            for (var y = 1; y <= map.Height; y++)
            for (var x = 1; x <= map.Width; x++)
            {
                var index = indices[x - 1, y - 1]; //-1 because indices are zero indexed.
                var tileset = Tileset.TilesetForPosition(index, map.Tilesets); //Tileset the index belongs to.

                var subTexturePosition =
                    index - tileset.FirstGid; //Resets position to start at 0 for the subtexture positioning.
                if (subTexturePosition <= -1) continue; //Don't render empty tiles.

                var worldpos =
                    Isometric.IsometricToWorld(new Point(x - 1, y - 1), map); //Isometric Projection to world coords.

                //Culling 
                if (worldpos.X > camera.bounds.x + camera.bounds.width || //Right side
                    worldpos.Y > camera.bounds.y + camera.bounds.height || //Bottom
                    worldpos.X < camera.bounds.x - map.LargestTileSize.X || //Left
                    worldpos.Y < camera.bounds.y - map.LargestTileSize.Y) //Top
                    continue; //Don't draw l


                var sourceRect = new Rectangle //Subposition in texture.
                {
                    X = subTexturePosition * tileset.TileWidth,
                    Y = (int) (Math.Floor(subTexturePosition / (double) tileset.Columns) *
                               tileset.TileHeight), //Y needs rounding.
                    Width = tileset.TileWidth,
                    Height = tileset.TileHeight
                };

                //Finally draw.
                graphics.batcher.draw(tileset.Texture, worldpos, sourceRect, Color.White, 0, new Vector2(0, 0), 1,
                    SpriteEffects.None, 0);
            }
        }
    }
}