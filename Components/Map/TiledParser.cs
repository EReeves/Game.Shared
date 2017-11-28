using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Shared.Utility;
using Microsoft.Xna.Framework.Content;
using Linq.Extras;
using Nez;
using Debug = System.Diagnostics.Debug;

namespace Game.Shared.Components.Map
{
    public class TiledParser
    {
        //<map .../> Attribute mapping. 
        private Dictionary<string, ReadXmlDelegate> mapAttributeMap;
        //<tileset .../> Attribute mapping. 
        private Dictionary<string, ReadTilesetDelegate> tilesetAttributeMap;
        //<xml/> Parent nodes
        private Dictionary<string, ReadXmlDelegate> nodeMap;
        //Used to bind the StrategyMap to the map/reader
        private delegate void ReadXmlDelegate(XmlReader reader, IsometricMap map);
        private delegate void ReadTilesetDelegate(XmlReader reader, Tileset tileset);

        public IsometricMap Load(ContentManager content, string filename)
        {
            InitializeStrategyMaps();
            var map = ParseXML($"{content.RootDirectory}{filename}");
            map.SortTilesets();
            map.LoadTextures(content);
            Isometric.Map = map;
            return map;
        }

        private void InitializeStrategyMaps()
        {
            mapAttributeMap = new Dictionary<string, ReadXmlDelegate>
            {
                {"width", (r, map) => { map.Width = r.ReadContentAsInt(); }},
                {"height", (r, map) => { map.Height = r.ReadContentAsInt(); }},
                {"tilewidth", (r, map) => { map.TileWidth = r.ReadContentAsInt(); }},
                {"tileheight", (r, map) => { map.TileHeight = r.ReadContentAsInt(); }}
            };
            
            tilesetAttributeMap = new Dictionary<string, ReadTilesetDelegate>
            {
                {"firstgid", (r, ts) => { ts.FirstGid = int.Parse(r.Value); }},
                {"source", (r, ts) => { ts.Source = r.Value.Split('.')[0]; }}
                
            };

            nodeMap = new Dictionary<string, ReadXmlDelegate>
            {
                {"map", ReadMapAttributes},
                {"tileset", ReadTilesetAttributes},
                {"layer", ReadLayerAttributes}
            };
        }

        //Reads <map .../> attributes
        private void ReadMapAttributes(XmlReader r, IsometricMap map)
        {
            while (r.MoveToNextAttribute())
                mapAttributeMap.GetValue(r.Name)?.Invoke(r, map);
        }

        //Reads <tileset .../> attributes
        private void ReadTilesetAttributes(XmlReader r, IsometricMap map)
        {
            var tileset = new Tileset();

            while (r.MoveToNextAttribute())
            {
                tilesetAttributeMap.TryGetValue(r.Name, out var action);
                Debug.Assert(action != null, nameof(action) + " != null");
                action.Invoke(r, tileset);
            }
            map.Tilesets.Add(tileset);
        }

        //Reads <layer .../> attributes.
        private static void ReadLayerAttributes(XmlReader r, IsometricMap map)
        {
            //Don't need a map, it's only one attriute and a subnode.
            r.MoveToAttribute("name");

            //It's a closing tag.
            if (!r.HasAttributes) return;

            //New layer
            var layer = new IsometricLayer(map.Width, map.Height)
            {
                Name = r.Value
            };

            r.MoveToElement();
            while (!r.Name.Equals("data") && r.NodeType != XmlNodeType.None) // format is <layer><dataforthelayer>
                r.Read();

            if (r.NodeType != XmlNodeType.Element) return;

            var mapIndices = r.ReadElementContentAsString()
                .Split(',')
                .Select(int.Parse)
                .ToArray(map.Width*map.Height); //parse csv indices, it's not complicated csv.

            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    layer.indices[x, y] = mapIndices[x + (y*map.Width)];
                }
            }

            map.Layers.Add(layer);
        }

        private IsometricMap ParseXML(string filename)
        {
            var map = new IsometricMap();

            using (var reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                    nodeMap.GetValue(reader.Name)?.Invoke(reader, map);
            }

            return map;
        }

       
    }
}