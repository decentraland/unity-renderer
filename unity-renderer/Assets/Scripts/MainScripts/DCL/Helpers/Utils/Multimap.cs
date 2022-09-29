using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCL
{
    public class Multimap<KT, VT>
    {
        private readonly Dictionary<KT, List<VT>> map = new Dictionary<KT, List<VT>>();

        public IEnumerable<KT> Keys
        {
            get { return map.Keys; }
        }

        public IEnumerable<VT> AllValues
        {
            get { return map.Values.SelectMany(list => list); }
        }

        public int Count
        {
            get { return map.Count; }
        }

        public int GetCountFor(KT key)
        {
            if (map.ContainsKey(key))
                return map[key].Count;
            return 0;
        }

        public int ValuesCount
        {
            get
            {
                int count = 0;
                foreach (var list in map.Values)
                {
                    count += list.Count;
                }

                return count;
            }
        }
        
        public IEnumerable<VT> GetValues(KT key)
        {
            return map.ContainsKey(key) ? map[key] : null;
        }
        
        public void Add(KT key, VT value)
        {
            if(map.ContainsKey(key))
                map[key].Add(value);
            else
            {
                map[key] = new List<VT>();
                map[key].Add(value);
            }
        }

        public void ReplaceFirst(KT key, VT value, Predicate<VT> predicate)
        {
            if (map.ContainsKey(key))
            {
                var list = map[key];
                var index = list.FindIndex(predicate);
                if (index != -1)
                    list[index] = value;
            }
        }

        public void Sort(KT key, Comparison<VT> comparison)
        {
            if(map.ContainsKey(key))
                map[key].Sort(comparison);
        }

        public void RemoveRange(KT key, int startIndex, int count)
        {
            if(map.ContainsKey(key))
                map[key].RemoveRange(startIndex, count);
        }
        
        public void Remove(KT key, VT value)
        {
            if (map.ContainsKey(key))
            {
                map[key].Remove(value);
                if (map[key].Count == 0)
                    map.Remove(key);
            }
        }

        public void RemoveFromEverywhere(VT value)
        {
            foreach (var kvp in map.ToList())
            {
                var list = kvp.Value;
                list.RemoveAll(v => v.Equals(value));
                if (list.Count == 0)
                    map.Remove(kvp.Key);
            }
        }

        public void Remove(KT key)
        {
            map.Remove(key);
        }

        public bool ContainsKey(KT key)
        {
            return map.ContainsKey(key);
        }

        public bool ContainsValue(VT value)
        {
            foreach (var list in map.Values)
            {
                if (list.Contains(value))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            map.Clear();
        }

        public void ClearLists()
        {
            foreach(var key in map.Keys)
                map[key].Clear();
        }
    }
}