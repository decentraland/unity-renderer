using System.Collections.Generic;

namespace DCL
{
    public class RefCountedMetric
    {
        private Dictionary<object, int> collection = new Dictionary<object, int>();

        public int GetObjectsCount()
        {
            return collection.Count;
        }

        public int GetRefCount(object obj)
        {
            if ( !collection.ContainsKey(obj) )
                return 0;

            return collection[obj];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool AddRef(object obj)
        {
            if ( obj == null )
                return false;

            if (!collection.ContainsKey(obj))
            {
                collection.Add(obj, 1);
                return true;
            }

            collection[obj]++;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool RemoveRef(object obj)
        {
            if ( obj == null )
                return true;

            if (!collection.ContainsKey(obj))
                return true;

            collection[obj]--;

            if (collection[obj] == 0)
            {
                collection.Remove(obj);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            collection.Clear();
        }
    }
}