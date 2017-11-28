using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.IEnumerableExtensions;

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
        
        public static Tileset TilesetForPosition(int pos, IList<Tileset> list)
        {
            for(var i=0;i<list.count();i++)
            {
                if (list.count() == 1 || list[i].FirstGid >= pos && list.count()-1 >= i + 1 && list[i + 1].FirstGid > pos)
                {
                    return list[i];
                }
            }
            return null;
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