using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Linq.Extras;
using Nez;
using Nez.Systems;

namespace Game.Shared.Components.Map
{
    public static class TiledParser
    {
        //Nodes, data and attributes are directed to these maps, parsed and put in to data structures.
        
        //<xml/>
        private static readonly Dictionary<string, ReadXmlDelegate> nodeMap =
            new Dictionary<string, ReadXmlDelegate>
            {
                ["map"] = ReadMapNode,
                ["tileset"] = ReadTilesetNode,
                ["layer"] = ReadLayerNode,
                ["objectgroup"] = ReadObjectGroupNode
            };
        //<map ...>
        private static readonly Dictionary<string, ReadXmlDelegate> mapParseMap =
            new Dictionary<string, ReadXmlDelegate>
            {
                ["width"] = (rdr, map) => { map.Width = rdr.ReadContentAsInt(); },
                ["height"] = (rdr, map) => { map.Height = rdr.ReadContentAsInt(); },
                ["tilewidth"] = (rdr, map) => { map.TileWidth = rdr.ReadContentAsInt(); },
                ["tileheight"] = (rdr, map) => { map.TileHeight = rdr.ReadContentAsInt(); }
            };
        //<tileset/>
        private static readonly Dictionary<string, ReadTilesetDelegate> tilesetParseMap =
            new Dictionary<string, ReadTilesetDelegate>
            {
                ["firstgid"] = (rdr, tileset) => { tileset.FirstGid = rdr.ReadContentAsInt(); },
                ["columns"] = (rdr, tileset) => { tileset.Columns = rdr.ReadContentAsInt(); },
                ["tilewidth"] = (rdr, tileset) => { tileset.TileWidth = rdr.ReadContentAsInt(); },
                ["tileheight"] = (rdr, tileset) => { tileset.TileHeight = rdr.ReadContentAsInt(); },
                ["image"] = (rdr, tileset) =>
                {
                    rdr.MoveToAttribute("source"); //don't bother with a map for one attribute.
                    tileset.Source = Path.GetFileNameWithoutExtension(rdr.Value);
                }
            };

        //<object/>
        private static readonly Dictionary<string, ReadObjectDelegate> objectParseMap =
            new Dictionary<string, ReadObjectDelegate>
            {
                ["id"] = (rdr, tiledObject) => { tiledObject.Id = rdr.ReadContentAsInt(); },
                ["name"] = (rdr, tiledObject) => { tiledObject.Name = rdr.Value; },
                ["x"] = (rdr, tiledObject) => { tiledObject.X = rdr.ReadContentAsFloat(); },
                ["y"] = (rdr, tiledObject) => { tiledObject.Y = rdr.ReadContentAsFloat(); },
                ["width"] = (rdr, tiledObject) => { tiledObject.Width = rdr.ReadContentAsFloat(); },
                ["height"] = (rdr, tiledObject) => { tiledObject.Height = rdr.ReadContentAsFloat(); }
            };

        //<objectgroup/>
        private static readonly Dictionary<string, ReadXmlDelegate> objectGroupParseMap =
             new Dictionary<string, ReadXmlDelegate>
            {
                ["name"] = (rdr, map) => { map.ObjectGroups.Add(rdr.Value, new List<TiledObject>()); },
                ["object"] = (rdr, map) =>
                {
                    var tObj = new TiledObject();
                    while (rdr.MoveToNextAttribute())
                    {
                        // ReSharper disable once InlineOutVariableDeclaration
                        ReadObjectDelegate value;
                        objectParseMap.TryGetValue(rdr.Name, out value);

                        if (value != null) value.Invoke(rdr, tObj); //It's a property we know about.
                        else tObj.Properties.Add(rdr.Name, rdr.Value); //Custom property.
                    }
                    //tiled objects are weird, just offset them to fit.
                    tObj.Y += map.TileHeight/2f;
                    tObj.X += map.TileHeight*1.5f;

                    map.ObjectGroups.AddObjectToEnd(tObj);
                }
            };

        //<layer/>
        private static readonly Dictionary<string, ReadXmlDelegate> layerParseMap =
            new Dictionary<string, ReadXmlDelegate>
            {
                ["name"] = (rdr, map) => { map.Layers.Last().Name = rdr.Value; },
                ["data"] = (rdr, map) =>
                {
                    //Parse and read indices in to layerdr.
                    var mapIndices = rdr.ReadElementContentAsString()
                        .Split(',')
                        .Select(int.Parse)
                        .ToArray(map.Width * map.Height); //parsing indices from csv, it's not complicated csv.

                    for (var y = 0; y < map.Height; y++)
                    for (var x = 0; x < map.Width; x++)
                        // ReSharper disable once PossibleMultipleEnumeration shut up
                        map.Layers.Last().indices[x, y] = mapIndices[x + y * map.Width];
                }
            };

        //Reads <map ...>
        private static void ReadMapNode(XmlReader rdr, IsometricMap map)
        {
            //Only iterate attributes, </map> end element is at the end of the file.
            while (rdr.MoveToNextAttribute())
            {
                mapParseMap.TryGetValue(rdr.Name, out var value);
                value?.Invoke(rdr, map);
            }
        }
        
       //Reads <tileset .../>
        private static void ReadTilesetNode(XmlReader rdr, IsometricMap map)
        {
            var tileset = new Tileset();

            foreach (var r in IterateNodeEnumerable(rdr))
            {
                tilesetParseMap.TryGetValue(r.Name, out var value);
                value?.Invoke(r, tileset);
            }

            map.Tilesets.Add(tileset);
        }

        //Reads <layer .../>
        private static void ReadLayerNode(XmlReader rdr, IsometricMap map)
        {
            map.Layers.Add(new IsometricLayer(map.Width, map.Height));

            foreach (var r in IterateNodeEnumerable(rdr))
            {
                layerParseMap.TryGetValue(r.Name, out var value);
                value?.Invoke(r, map);
            }
        }

        private static void ReadObjectGroupNode(XmlReader rdr, IsometricMap map)
        {
            foreach (var r in IterateNodeEnumerable(rdr))
            {
                objectGroupParseMap.TryGetValue(r.Name, out var value);
                value?.Invoke(r, map);
            }
        }

        private static IsometricMap ParseXML(string filename)
        {
            var map = IsometricMap.Instance;

            using (var rdr = XmlReader.Create(filename))
            {
                while (ReadNext(rdr))
                {
                    if (rdr.NodeType == XmlNodeType.EndElement) continue;
                    nodeMap.TryGetValue(rdr.Name, out var value);
                    value?.Invoke(rdr, map);
                }
            }

            return map;
        }

        //Read without all the garbage.
        private static bool ReadNext(XmlReader rdr)
        {
            //If it's whitespace or garbage keep reading.
            XmlNodeType n;
            bool hasNext;
            do
            {
                hasNext = rdr.Read();
                n = rdr.NodeType;
            } while (hasNext && n != XmlNodeType.Element && n != XmlNodeType.EndElement && n != XmlNodeType.Attribute);
            return hasNext;
        }

        /// <summary>
        ///     Iterate over everything within a node including data.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private static IEnumerable<XmlReader> IterateNodeEnumerable(XmlReader rdr)
        {
            while (rdr.NodeType != XmlNodeType.EndElement)
            {
                if (!rdr.MoveToNextAttribute()) ReadNext(rdr); //Move to attribute if applicable, else read next line.
                yield return rdr;
            }
        }

        public static IsometricMap Load(this NezContentManager content, string path)
        {
            var map = ParseXML($"{content.RootDirectory}{path}");
            map.SortTilesets();
            map.LoadTextures(content);
            return map;
        }

        //Data bound to maps.

        private delegate void ReadXmlDelegate(XmlReader readerdr, IsometricMap map);

        private delegate void ReadTilesetDelegate(XmlReader readerdr, Tileset tileset);

        private delegate void ReadObjectDelegate(XmlReader readerdr, TiledObject tiledObject);
    }
}