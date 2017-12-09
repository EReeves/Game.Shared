using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Game.Shared.Components.Map
{
    public sealed class ObjectGroups : Dictionary<string, List<TiledObject>>
    {
        private string lastKey;

        //Keep track of the last key added, way more convenient when parsing lists of items.
        //Sealed the class cause Add is being hidden..
        public new void Add(string key, List<TiledObject> value)
        {
            lastKey = key;
            base.Add(key, value);
        }

        /// <summary>
        /// Adds a TiledObject to the last List added to the dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddObjectToEnd(TiledObject value)
        {
            this[lastKey].Add(value);
        }
        


    }
}