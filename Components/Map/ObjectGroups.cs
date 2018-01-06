using System.Collections.Generic;
using System.Linq;

namespace Game.Shared.Components.Map
{
    public sealed class ObjectGroups : Dictionary<string, List<TiledObject>>
    {
        private string lastKey;

        //Keep track of the last key added, way more convenient when parsing lists of items.
        //Sealed the class cause Add is being hidden..
        public new void Add(string key, List<TiledObject> value)
        {
            if (ContainsKey(key)) return; //Ignore duplicates.
            lastKey = key;
            base.Add(key, value);
        }

        /// <summary>
        ///     Adds a TiledObject to the last List added to the dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddObjectToEnd(TiledObject value)
        {
            this[lastKey].Add(value);
        }

        public TiledObject ObjectAtEnd()
        {
            if (Count == 0) return null;
            return this[lastKey].Last();
        }
    }

    public static class ObjectGroupExtensions
    {
        public static TiledObject GetTiledObject(this List<TiledObject> list, string name)
        {
            return list.First(a => a.Name.Equals(name));
        }
    }
}