using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Game.Shared.Utility;
using Linq.Extras;
using Nez.Systems;

namespace Game.Shared.Components.Map
{
    public static class TiledParser
    {
        //Nodes, data and attributes are directed to these maps, parsed and put in to data structures.

        private static readonly Dictionary<string, ReadXmlDelegate> nodeMap =
            new Dictionary<string, ReadXmlDelegate>
            {
                ["map"] = ReadMapNode,
                ["tileset"] = ReadTilesetNode,
                ["layer"] = ReadLayerNode,
                ["objectgroup"] = ReadObjectGroupNode
            };

        private static readonly Dictionary<string, ReadXmlDelegate> mapParseMap =
            new Dictionary<string, ReadXmlDelegate>
            {
                ["width"] = (r, map) => { map.Width = r.ReadContentAsInt(); },
                ["height"] = (r, map) => { map.Height = r.ReadContentAsInt(); },
                ["tilewidth"] = (r, map) => { map.TileWidth = r.ReadContentAsInt(); },
                ["tileheight"] = (r, map) => { map.TileHeight = r.ReadContentAsInt(); }
            };

        private static readonly Dictionary<string, ReadTilesetDelegate> tilesetParseMap =
            new Dictionary<string, ReadTilesetDelegate>
            {
                ["firstgid"] = (r, tileset) => { tileset.FirstGid = r.ReadContentAsInt(); },
                ["columns"] = (r, tileset) => { tileset.Columns = r.ReadContentAsInt(); },
                ["tilewidth"] = (r, tileset) => { tileset.TileWidth = r.ReadContentAsInt(); },
                ["tileheight"] = (r, tileset) => { tileset.TileHeight = r.ReadContentAsInt(); },
                ["image"] = (r, tileset) =>
                {
                    r.MoveToAttribute("source"); //don't bother with a map for one attribute.
                    tileset.Source = Path.GetFileNameWithoutExtension(r.Value);
                }
            };

        private static readonly Dictionary<string, ReadObjectDelegate> objectParseMap =
            new Dictionary<string, ReadObjectDelegate>
            {
                ["id"] = (r, tiledObject) => { tiledObject.Id = r.ReadContentAsInt(); },
                ["name"] = (r, tiledObject) => { tiledObject.Name = r.Value; },
                ["x"] = (r, tiledObject) => { tiledObject.X = r.ReadContentAsFloat(); },
                ["y"] = (r, tiledObject) => { tiledObject.Y = r.ReadContentAsFloat(); },
                ["width"] = (r, tiledObject) => { tiledObject.Width = r.ReadContentAsFloat(); },
                ["height"] = (r, tiledObject) => { tiledObject.Height = r.ReadContentAsFloat(); }
            };

        private static readonly Dictionary<string, ReadObjectGroupDelegate> objectGroupParseMap =
            new Dictionary<string, ReadObjectGroupDelegate>
            {
                ["name"] = (r, objectGrp) => { objectGrp.Add(r.Value, new List<TiledObject>()); },
                ["object"] = (r, objectGrp) =>
                {
                    var tObj = new TiledObject();
                    while (r.MoveToNextAttribute())
                    {
                        objectParseMap.TryGetValue(r.Name, out var value);

                        if (value != null) value.Invoke(r, tObj); //It's a property we know about.
                        else tObj.Properties.Add(r.Name, r.Value); //Custom property.
                    }

                    objectGrp.AddObjectToEnd(tObj);
                }
            };

        private static readonly Dictionary<string, ReadXmlDelegate> layerParseMap =
            new Dictionary<string, ReadXmlDelegate>
            {
                ["name"] = (r, map) => { map.Layers.Last().Name = r.Value; },
                ["data"] = (r, map) =>
                {
                    //Parse and read indices in to layer.
                    var mapIndices = r.ReadElementContentAsString()
                        .Split(',')
                        .Select(int.Parse)
                        .ToArray(map.Width * map.Height); //parse csv indices, it's not complicated csv.

                    for (var y = 0; y < map.Height; y++)
                    for (var x = 0; x < map.Width; x++)
                        // ReSharper disable once PossibleMultipleEnumeration
                        map.Layers.Last().indices[x, y] = mapIndices[x + y * map.Width];
                }
            };

        //Reads <map .../>
        private static void ReadMapNode(XmlReader r, IsometricMap map)
        {
            //Only iterate attributes, map endelement is at the end of the file.
            while (r.MoveToNextAttribute())
            {
                mapParseMap.TryGetValue(r.Name, out var value);
                value?.Invoke(r, map);
            }
            ;
        }

        //Reads <tileset .../>
        private static void ReadTilesetNode(XmlReader r, IsometricMap map)
        {
            var tileset = new Tileset();

            foreach (var rdr in IterateNodeEnumerable(r))
            {
                tilesetParseMap.TryGetValue(rdr.Name, out var value);
                value?.Invoke(rdr, tileset);
            }
            map.Tilesets.Add(tileset);
        }

        //Reads <layer .../>
        private static void ReadLayerNode(XmlReader r, IsometricMap map)
        {
            map.Layers.Add(new IsometricLayer(map.Width, map.Height));

            foreach (var rdr in IterateNodeEnumerable(r))
            {
                layerParseMap.TryGetValue(rdr.Name, out var value);
                value?.Invoke(rdr, map);
            }
        }

        private static void ReadObjectGroupNode(XmlReader r, IsometricMap map)
        {
            foreach (var rdr in IterateNodeEnumerable(r))
            {
                objectGroupParseMap.TryGetValue(rdr.Name, out var value);
                value?.Invoke(rdr, map.ObjectGroups);
            }
        }

        private static IsometricMap ParseXML(string filename)
        {
            var map = new IsometricMap();

            using (var r = XmlReader.Create(filename))
            {
                while (ReadNext(r))
                {
                    if (r.NodeType == XmlNodeType.EndElement) continue;
                    nodeMap.TryGetValue(r.Name, out var value);
                    value?.Invoke(r, map);
                }
            }

            return map;
        }

        //Read without all the garbage.
        private static bool ReadNext(XmlReader r)
        {
            //If it's whitespace or garbage keep reading.
            XmlNodeType n;
            bool hasNext;
            do
            {
                hasNext = r.Read();
                n = r.NodeType;
            } while (hasNext && n != XmlNodeType.Element && n != XmlNodeType.EndElement && n != XmlNodeType.Attribute);
            return hasNext;
        }

        /// <summary>
        ///     Iterate over everything within a node including data.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private static IEnumerable<XmlReader> IterateNodeEnumerable(XmlReader r)
        {
            while (r.NodeType != XmlNodeType.EndElement)
            {
                if (!r.MoveToNextAttribute()) ReadNext(r); //Move to attribute if applicable, else read next line.
                yield return r;
            }
        }

        public static IsometricMap Load(this NezContentManager content, string path)
        {
            var map = ParseXML($"{content.RootDirectory}{filename}");
            map.SortTilesets();
            map.LoadTextures(content);
            Isometric.Map = map;
            return map;
        }

        //Data bound to maps.

        private delegate void ReadXmlDelegate(XmlReader reader, IsometricMap map);

        private delegate void ReadTilesetDelegate(XmlReader reader, Tileset tileset);

        private delegate void ReadObjectGroupDelegate(XmlReader reader, ObjectGroups objectGroups);

        private delegate void ReadObjectDelegate(XmlReader reader, TiledObject tiledObject);
    }
}