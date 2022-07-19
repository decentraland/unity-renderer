using System;
using System.Collections.Generic;

namespace DCL.CRDT
{
    public class CRDTProtocol
    {
        internal readonly List<CRDTMessage> state = new List<CRDTMessage>();
        private readonly Dictionary<long, int> stateIndexer = new Dictionary<long, int>();
        private bool clearOnUpdated = false;

        public CRDTMessage ProcessMessage(CRDTMessage message)
        {
            if (clearOnUpdated)
            {
                Clear();
            }

            TryGetState(message.key, out CRDTMessage storedMessage);

            // The received message is > than our current value, update our state.
            if (storedMessage == null || storedMessage.timestamp < message.timestamp)
            {
                return UpdateState(message.key, message.data, message.timestamp);
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

            return UpdateState(message.key, message.data, message.timestamp);
        }

        public CRDTMessage GetState(long key)
        {
            if (stateIndexer.TryGetValue(key, out int index))
            {
                return state[index];
            }
            return null;
        }

        public bool TryGetState(long key, out CRDTMessage crdtMessage)
        {
            if (stateIndexer.TryGetValue(key, out int index))
            {
                crdtMessage = state[index];
                return true;
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
                key = CRDTUtils.KeyFromIds(entityId, componentId),
                data = data,
                timestamp = 0
            };
            if (TryGetState(result.key, out CRDTMessage storedMessage))
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

        private CRDTMessage UpdateState(long key, object data, long remoteTimestamp)
        {
            long stateTimeStamp = 0;
            bool containState = stateIndexer.TryGetValue(key, out int keyIndex);

            if (containState)
            {
                stateTimeStamp = state[keyIndex].timestamp;
            }

            long timestamp = Math.Max(remoteTimestamp, stateTimeStamp);
            var newMessageState = new CRDTMessage()
            {
                key = key,
                timestamp = timestamp,
                data = data
            };

            if (containState)
            {
                state[keyIndex] = newMessageState;
            }
            else
            {
                state.Add(newMessageState);
                stateIndexer.Add(key, state.Count - 1);
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