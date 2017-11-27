using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Shared.Components.Map
{
    public class Tileset
    {
        public Tileset()
        {
            
        }

        public Tileset(string source)
        {
            Source = source;
        }
        
        //Tileset filename
        public string Source { get; set; }

        //e.g. firstGuid - another tilesets firstguid is the indies applicable to this tileset.
        public int FirstGid { get; set; }

        public Texture2D Texture { get; set; }
        
        public static Tileset TilesetForPosition(int pos, IEnumerable<Tileset> list)
        {
            return list.First(a => a.FirstGid >= pos);
        }

        public static void LoadTextures(ContentManager content, IEnumerable<Tileset> list)
        {
            foreach (var tileset in list)
            {
                tileset.Texture = content.Load<Texture2D>(tileset.Source);
            }
        }

    }
}