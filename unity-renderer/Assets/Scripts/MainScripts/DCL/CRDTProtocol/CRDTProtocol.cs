using System;
using System.Collections.Generic;

namespace DCL.CRDT
{
    public class CRDTProtocol
    {
        internal readonly List<CRDTMessage> state = new List<CRDTMessage>();
        private readonly Dictionary<int, Dictionary<int, int>> stateIndexer = new Dictionary<int, Dictionary<int, int>>();

        private bool clearOnUpdated = false;

        public CRDTMessage ProcessMessage(CRDTMessage message)
        {
            if (clearOnUpdated)
            {
                Clear();
            }
            
            TryGetState(message.key1, message.key2, out CRDTMessage storedMessage);

            // The received message is > than our current value, update our state.
            if (storedMessage == null || storedMessage.timestamp < message.timestamp)
            {
                return UpdateState(message.key1, message.key2, message.data, message.timestamp);
            }

            // Outdated Message. Resend our state message through the wire.
            if (storedMessage.timestamp > message.timestamp)
            {
                return storedMessage;
            }

            // Same data, same timestamp. Weirdo echo message.
            if (IsSameData(storedMessage.data, message.data))
            {
                return storedMessage;
            }

            // Race condition, same timestamp diff data. Should keep stored data?
            if (CompareData(storedMessage.data, message.data))
            {
                return storedMessage;
            }

            return UpdateState(message.key1, message.key2, message.data, message.timestamp);
        }

        public CRDTMessage GetState(int key1, int key2)
        {
            TryGetState(key1, key2, out CRDTMessage crdtMessage);
            return crdtMessage;
        }

        public bool TryGetState(int key1, int key2, out CRDTMessage crdtMessage)
        {
            if (stateIndexer.TryGetValue(key1, out Dictionary<int, int> innerDictionary))
            {
                if (innerDictionary.TryGetValue(key2, out int index))
                {
                    crdtMessage = state[index];
                    return true;
                }
            }
            crdtMessage = null;
            return false;
        }

        public IReadOnlyList<CRDTMessage> GetState()
        {
            return state;
        }

        public CRDTMessage Create(int entityId, int componentId, byte[] data)
        {
            var result = new CRDTMessage()
            {
                key1 = entityId,
                key2 = componentId,
                data = data,
                timestamp = 0
            };
            if (TryGetState(result.key1, result.key2, out CRDTMessage storedMessage))
            {
                result.timestamp = storedMessage.timestamp + 1;
            }
            return result;
        }

        public void Clear()
        {
            state.Clear();
            stateIndexer.Clear();
            clearOnUpdated = false;
        }

        public void ClearOnUpdated()
        {
            clearOnUpdated = true;
        }

        private CRDTMessage UpdateState(int key1, int key2, object data, long remoteTimestamp)
        {
            long stateTimeStamp = 0;
            int crdtStateIndex = 0;
            bool stateExists = false;

            if (stateIndexer.TryGetValue(key1, out Dictionary<int, int> innerDictionary))
            {
                stateExists = innerDictionary.TryGetValue(key2, out crdtStateIndex);
            }

            if (stateExists)
            {
                stateTimeStamp = state[crdtStateIndex].timestamp;
            }

            long timestamp = Math.Max(remoteTimestamp, stateTimeStamp);
            var newMessageState = new CRDTMessage()
            {
                key1 = key1,
                key2 = key2,
                timestamp = timestamp,
                data = data
            };

            if (stateExists)
            {
                state[crdtStateIndex] = newMessageState;
            }
            else
            {
                state.Add(newMessageState);
                int newStateIndex = state.Count - 1;
                if (innerDictionary != null)
                {
                    innerDictionary.Add(key2, newStateIndex);
                }
                else
                {
                    stateIndexer[key1] = new Dictionary<int, int>() { { key2, newStateIndex } };
                }
            }

            return newMessageState;
        }

        internal static bool IsSameData(object a, object b)
        {
            if (a == b)
            {
                return true;
            }

            if (a is byte[] bytesA && b is byte[] bytesB)
            {
                if (bytesA.Length != bytesB.Length)
                {
                    return false;
                }

                for (int i = 0; i < bytesA.Length; i++)
                {
                    if (bytesA[i] != bytesB[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            if (a is string strA && b is string strB)
            {
                return String.Compare(strA, strB, StringComparison.Ordinal) == 0;
            }

            return false;
        }

        private static bool CompareData(object a, object b)
        {
            if (a is byte[] bytesA && b is byte[] bytesB)
            {
                return bytesA.Length > bytesB.Length;
            }

            if (a is int numberA && b is int numberB)
            {
                return numberA > numberB;
            }

            if (a is string strA && b is string strB)
            {
                return String.Compare(strA, strB, StringComparison.Ordinal) > 0;
            }

            return true;
        }
    }
}