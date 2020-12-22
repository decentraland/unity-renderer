using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    public class CompositeLock
    {
        public static bool VERBOSE = false;

        public event System.Action OnAllLocksRemoved;

        int lockCounter = 0;
        HashSet<object> lockIds = new HashSet<object>();

        public bool isUnlocked => lockCounter == 0;

        public void AddLock(object id)
        {
            if (lockIds.Contains(id))
                return;

            lockIds.Add(id);
            lockCounter++;

            if (VERBOSE)
                Debug.Log($"Lock added... {lockCounter}");
        }

        public void RemoveLock(object id)
        {
            if (!lockIds.Contains(id))
                return;

            lockIds.Remove(id);
            lockCounter--;

            if (VERBOSE)
                Debug.Log($"Locking removed... {lockCounter}");

            if (lockCounter == 0)
                OnAllLocksRemoved?.Invoke();
        }

        public void RemoveAllLocks()
        {
            lockCounter = 0;
            lockIds.Clear();
            OnAllLocksRemoved?.Invoke();
        }

        public HashSet<object> GetLockIdsCopy()
        {
            return new HashSet<object>(lockIds);
        }
    }
}
