using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Game.Shared.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Nez;

namespace Game.Shared.Components.Map
{
    public class IsometricMap
    {
        //Singleton.
        private static IsometricMap instance;

        private bool tilesetsSorted;

        private IsometricMap()
        {
        }

        public static IsometricMap Instance
        {
            get
            {
                instance = instance ?? new IsometricMap();
                return instance;
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public Vector2 LargestTileSize { get; private set; }
        public OverlapZones OverlapZones { get; private set; }
        
        public DenseArray<TileProperties> TileProperties { get; set; }

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
            var x = 0;
            var y = 0;
            foreach (var tileset in Tilesets)
            {
                x = tileset.TileWidth > x ? tileset.TileWidth : x;
                y = tileset.TileHeight > y ? tileset.TileHeight : y;
            }

            LargestTileSize = new Vector2(x, y);
        }

        public void LoadTextures(ContentManager content)
        {
            Assert.IsTrue(tilesetsSorted,
                "Tilesets must be sorted before they can be used. See IsometricMap.SortTilesets()");
            Tileset.LoadTextures(content, Tilesets);
        }

        public void CalculateOverlapZones(Entity entity)
        {
            OverlapZones = new OverlapZones(this, entity);
        }
        
        private static readonly Dictionary<string, Func<string, TileProperties, TileProperties>> TilePropertyMap = 
            new Dictionary<string, Func<string, TileProperties, TileProperties>>
        {
            ["ShouldCollide"] = (stringValue, properties) =>
            {
                // ReSharper disable once InlineOutVariableDeclaration
                bool result;
                if(bool.TryParse(stringValue, out result));
                    properties.ShouldCollide = result;

                return properties;
            },
        };

        public void CalculateTileProperties()
        {
            //Set tile properties to the same size as the map.
            TileProperties = new DenseArray<TileProperties>(Width, Height);

            //For each item
            var properties = ObjectGroups["TileProperties"];
            foreach (var tiledObject in properties)
            {
                //Create a new item.
                var tileProperties = new TileProperties();

               //Parse properties
                tileProperties = tiledObject.Properties
                    .Aggregate(tileProperties, (current, tiledObjectProperty) =>
                        TilePropertyMap[tiledObjectProperty.Key].Invoke(tiledObjectProperty.Value, current));

                //find prositon and store.
                var pos = Isometric.WorldToIsometric(tiledObject.WorldPosition, this);
                TileProperties[(int) pos.X, (int) pos.Y] = tileProperties;
            }
        }
    }
}