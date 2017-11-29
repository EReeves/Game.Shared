using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Nez;

namespace Game.Shared.Components.Map
{
    public class IsometricMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public Vector2 LargestTileSize { get; private set; }
        private bool tilesetsSorted = false;

        //5 should do for now. Use a list because we will probably have multiple maps.
        public List<Tileset> Tilesets { get; set; } = new List<Tileset>(5);
        public List<IsometricLayer> Layers { get; set; } = new List<IsometricLayer>(5);


        public void SortTilesets()
        {
            Tilesets.Sort((a, b) => a.FirstGid.CompareTo(b.FirstGid));
            tilesetsSorted = true;

            //Set largest tile size, used for culling
            var x = 0; var y = 0;
            foreach (var tileset in Tilesets)
            {
                x = tileset.TileWidth > x ? tileset.TileWidth : x;
                y = tileset.TileHeight > y ? tileset.TileHeight : y;
            }
            LargestTileSize = new Vector2(x,y);
        }

        public void LoadTextures(ContentManager content)
        {
            Assert.isTrue(tilesetsSorted, "Tilesets must be sorted before they can be used. See IsometricMap.SortTilesets()");
            Tileset.LoadTextures(content, Tilesets);
        }
    }
}