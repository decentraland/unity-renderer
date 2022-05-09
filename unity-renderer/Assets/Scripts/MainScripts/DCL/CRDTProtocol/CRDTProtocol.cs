using System;
using System.Collections.Generic;

namespace DCL.CRDT
{
    public class CRDTProtocol
    {
        internal readonly Dictionary<long, CRDTMessage> state = new Dictionary<long, CRDTMessage>();

        public CRDTMessage ProcessMessage(CRDTMessage message)
        {
            state.TryGetValue(message.key, out CRDTMessage storedMessage);

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

        public CRDTMessage GetSate(long key)
        {
            state.TryGetValue(key, out CRDTMessage storedMessage);
            return storedMessage;
        }

        private CRDTMessage UpdateState(long key, object data, double remoteTimestamp)
        {
            double stateTimeStamp = 0;
            if (state.TryGetValue(key, out CRDTMessage storedMessage))
            {
                stateTimeStamp = storedMessage.timestamp;
            }

            double timestamp = Math.Max(remoteTimestamp, stateTimeStamp);
            var newMessageState = new CRDTMessage()
            {
                key = key,
                timestamp = timestamp,
                data = data
            };

            state[key] = newMessageState;

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