using System;
using System.Collections.Generic;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Game.Shared.Components.Map
{
    public class IsometricLayer : RenderableComponent
    {
        public const float LayerDepthBack = 0f;
        public const float LayerDepthObjback = 0.3f;
        public const float LayerDepthObjfront = 0.4f;
        public const float LayerDepthFront = 0.9f;
        private readonly IsometricMap map;
        
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public IsometricLayer(int sizex, int sizey, bool isObjectPositioningLayer = false)
        {
            Indices = new DenseArray<int>(sizey, sizex);
            map = IsometricMap.Instance;

            if (isObjectPositioningLayer)
                SetAsObjectPositioningLayer();
            
        }

        public bool IsObjectPositioningLayer { get; private set; }
        public DenseArray<int> Indices { get; }
        public string Name { get; set; }

        public void SetAsObjectPositioningLayer()
        {
            IsObjectPositioningLayer = true;
            SetRenderLayer(map.ObjectRenderLayer);
            SetLayerDepth(LayerDepthObjfront);
        }

        /// <summary>
        /// Should be run after layer is populated, will replace layer depth with specified value if specified.
        /// </summary>
        public void CheckForPropertyDepth()
        {
            if (Properties.TryGetValue("depth", out var depth))
            {
                float.TryParse(depth, out var floatValue);
                SetLayerDepth(floatValue);
            }
        }

        public static int ObjectRenderLayerPosition(IList<IsometricLayer> list)
        {
            const string objectPositioningLayerName = "ObjectPositioningLayer";
            for (var i = 0; i < list.Count; i++)
                if (list[i].Name == objectPositioningLayerName)
                    return Isometric.RenderLayerStart - i;
            throw new Exception($"{objectPositioningLayerName} does not exist in tmx file.");
        }

        public override bool IsVisibleFromCamera(Camera camera)
        {
            //This is the main map class, it will always be drawn unless manually disabled.
            return true;
        }

        public override void Render(Graphics graphics, Camera camera)
        {
            for (var y = 1; y <= map.Height; y++)
            for (var x = 1; x <= map.Width; x++)
            {
          
                var index = Indices[x - 1, y - 1]; //-1 because indices are zero indexed.
                var tileset = Tileset.TilesetForPosition(index, map.Tilesets); //Tileset the index belongs to.

                var subTexturePosition =
                    index - tileset.FirstGid; //Resets position to start at 0 for the subtexture positioning.
                if (subTexturePosition <= -1) continue; //Don't render empty tiles.

                var worldpos =
                    Isometric.IsometricToWorld(new Point(x - 1, y - 1), map); //Isometric Projection to world coords.

                //Culling 
                if (worldpos.X > camera.Bounds.X + camera.Bounds.Width || //Right side
                    worldpos.Y > camera.Bounds.Y + camera.Bounds.Height || //Bottom
                    worldpos.X < camera.Bounds.X - map.LargestTileSize.X || //Left
                    worldpos.Y < camera.Bounds.Y - map.LargestTileSize.Y) //Top
                    continue; //Don't draw l


                var columnRounding = subTexturePosition % tileset.Columns;
                var sourceRect = new RectangleF //Subposition in texture.
                {
                    X = (columnRounding * tileset.TileWidth),
                    Y = (subTexturePosition / tileset.Columns) *
                               tileset.TileHeight, //Y needs rounding.
                    Width = tileset.TileWidth,
                    Height = tileset.TileHeight
                };

                //Finally draw.
                graphics.Batcher.Draw(tileset.Texture, worldpos, sourceRect, Color.White, 0, new Vector2(0, 0), 1,SpriteEffects.None, 0);
                
            }
        }
    }
}