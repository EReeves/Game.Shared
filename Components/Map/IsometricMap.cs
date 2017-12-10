using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Nez;

namespace Game.Shared.Components.Map
{
    public class IsometricMap
    {
        //Singleton.
        private static IsometricMap instance;
        public static IsometricMap Instance
        {
            get
            {
                instance = instance ?? new IsometricMap();
                return instance;
            }
        }
        private IsometricMap() {}

        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public Vector2 LargestTileSize { get; private set; }
        private bool tilesetsSorted = false;
        public OverlapZones OverlapZones { get; private set; }

        
        //5 should do for now. Use a list because we will probably have multiple maps.
        public List<Tileset> Tilesets { get; set; } = new List<Tileset>(5);
        public List<IsometricLayer> Layers { get; set; } = new List<IsometricLayer>(5);
        public ObjectGroups ObjectGroups { get; set; } = new ObjectGroups();

        public int ObjectRenderLayer => IsometricLayer.ObjectRenderLayerPosition(Layers);
        //TODO: should probably load this from a layer like ObjectRenderLayer.
        public int ObjectRenderLayerEnd => ObjectRenderLayer + 10;

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

        public void CalculateOverlapZones(Entity entity)
        {
            OverlapZones = new OverlapZones(this, entity);
        }
    }
}