using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL
{
    public class Multimap<KT, VT>
    {
        private readonly Dictionary<KT, List<VT>> _map = new Dictionary<KT, List<VT>>();

        public IEnumerable<KT> Keys
        {
            get { return _map.Keys.Select(key => key); }
        }

        public int Count
        {
            get { return _map.Count; }
        }

        public int GetCountFor(KT key)
        {
            if (_map.ContainsKey(key))
                return _map[key].Count;
            return 0;
        }

        public int ValuesCount
        {
            get
            {
                int count = 0;
                foreach (var list in _map.Values)
                {
                    count += list.Count;
                }

                return count;
            }
        }

        public IEnumerable<VT> AllValues
        {
            get { return _map.Values.SelectMany(list => list); }
        }
        
        public IEnumerable<VT> GetValues(KT key)
        {
            return _map.ContainsKey(key) ? _map[key] : null;
        }
        
        public void Add(KT key, VT value)
        {
            if(_map.ContainsKey(key))
                _map[key].Add(value);
            else
            {
                _map[key] = new List<VT>();
                _map[key].Add(value);
            }
        }

        public void ReplaceFirst(KT key, VT value, Predicate<VT> predicate)
        {
            if (_map.ContainsKey(key))
            {
                var list = _map[key];
                var index = list.FindIndex(predicate);
                if (index != -1)
                    list[index] = value;
            }
        }

        public void Sort(KT key, Comparison<VT> comparison)
        {
            if(_map.ContainsKey(key))
                _map[key].Sort(comparison);
        }

        public void RemoveRange(KT key, int startIndex, int count)
        {
            if(_map.ContainsKey(key))
                _map[key].RemoveRange(startIndex, count);
        }
        
        public void Remove(KT key, VT value)
        {
            if (_map.ContainsKey(key))
            {
                _map[key].Remove(value);
                if (_map[key].Count == 0)
                    _map.Remove(key);
            }
        }

        public void RemoveFromEverywhere(VT value)
        {
            foreach (var kvp in _map.ToList())
            {
                var list = kvp.Value;
                list.RemoveAll(v => v.Equals(value));
                if (list.Count == 0)
                    _map.Remove(kvp.Key);
            }
        }

        public void Remove(KT key)
        {
            _map.Remove(key);
        }

        public bool ContainsKey(KT key)
        {
            return _map.ContainsKey(key);
        }

        public bool ContainsValue(VT value)
        {
            foreach (var list in _map.Values)
            {
                if (list.Contains(value))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            _map.Clear();
        }

        public void ClearLists()
        {
            foreach(var key in _map.Keys)
                _map[key].Clear();
        }
    }
}