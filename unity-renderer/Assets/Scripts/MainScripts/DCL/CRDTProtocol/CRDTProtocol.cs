using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL.CRDT
{
    public class CRDTProtocol
    {

        public enum ProcessMessageResultType
        {
            /**
           * Typical message and new state set.
           * @state CHANGE
           * @reason Incoming message has a timestamp greater
           */
            StateUpdatedTimestamp = 1,

            /**
           * Typical message when it is considered old.
           * @state it does NOT CHANGE.
           * @reason incoming message has a timestamp lower.
           */
            StateOutdatedTimestamp = 2,

            /**
           * Weird message, same timestamp and data.
           * @state it does NOT CHANGE.
           * @reason consistent state between peers.
           */
            NoChanges = 3,

            /**
           * Less but typical message, same timestamp, resolution by data.
           * @state it does NOT CHANGE.
           * @reason incoming message has a LOWER data.
           */
            StateOutdatedData = 4,

            /**
           * Less but typical message, same timestamp, resolution by data.
           * @state CHANGE
           * @reason incoming message has a GREATER data.
           */
            StateUpdatedData = 5,

            /**
           * Entity was previously deleted.
           * @state it does NOT CHANGE.
           * @reason The message is considered old.
           */
            EntityWasDeleted = 6,

            /**
           * Entity should be deleted.
           * @state CHANGE
           * @reason the state is storing old entities
           */
            EntityDeleted = 7,

            /**
           * An APPEND with a success insert
           * @state CHANGE
           * @reason the element is not in set set yet
           */
            StateElementAddedToSet = 8,
        };

        private int MAX_ELEMENT_SET = 100;

        public class CrdtEntityComponentData
        {
            public long entityId;
            public int componentId;
            public int timestamp;
            public object data;
        }

        public class EntityComponentData: IComparable<EntityComponentData>
        {
            public int timestamp;
            public object data;

            public int CompareTo(EntityComponentData other)
            {
                int timestampDiff = this.timestamp - other.timestamp;

                if (timestampDiff == 0)
                {
                    return CompareData(this.data, other.data);
                }
                else
                {
                    return Math.Sign(timestampDiff);
                }
            }
        }

        public class CrdtState
        {
            public readonly Dictionary<int, Dictionary<long, EntityComponentData>> singleComponents = new Dictionary<int, Dictionary<long, EntityComponentData>>();
            public readonly Dictionary<int, Dictionary<long, List<EntityComponentData>>> setComponents = new Dictionary<int, Dictionary<long, List<EntityComponentData>>>();
            public readonly Dictionary<int, int> deletedEntitiesSet = new Dictionary<int, int>();
        }

        private CrdtState state = new CrdtState();

        private bool clearOnUpdated = false;

        public ProcessMessageResultType ProcessMessage(CRDTMessage message)
        {
            if (clearOnUpdated)
            {
                Clear();
            }

            int entityId = (int)message.entityId;
            int entityNumber = entityId & 0xffff;
            int entityVersion = (entityId >> 16) & 0xffff;
            bool entityNumberWasDeleted = state.deletedEntitiesSet.TryGetValue(entityNumber, out int deletedVersion);

            if (entityNumberWasDeleted)
            {
                if (deletedVersion >= entityVersion)
                {
                    return ProcessMessageResultType.EntityWasDeleted;
                }
            }

            if (message.type == CrdtMessageType.DELETE_ENTITY)
            {
                if (entityNumberWasDeleted)
                {
                    state.deletedEntitiesSet[entityNumber] = entityVersion;
                }
                else
                {
                    state.deletedEntitiesSet.Add(entityNumber, entityVersion);
                }

                foreach (var component in state.singleComponents)
                {
                    component.Value.Remove(entityId);
                }

                foreach (var componentSet in state.setComponents)
                {
                    componentSet.Value.Remove(entityId);
                }

                // TODO: clean the state with this entityId

                return ProcessMessageResultType.EntityDeleted;
            }

            if (message.type == CrdtMessageType.APPEND_COMPONENT)
            {
                bool elementAdded = TryAddSetComponentState(message.entityId, message.componentId, message.data, message.timestamp);

                if (elementAdded)
                {
                    return ProcessMessageResultType.StateElementAddedToSet;
                }
                else
                {
                    return ProcessMessageResultType.NoChanges;
                }
            }

            TryGetSingleComponentState(message.entityId, message.componentId, out EntityComponentData storedData);

            // The received message is > than our current value, update our state.components.
            if (storedData == null || storedData.timestamp < message.timestamp)
            {
                UpdateSingleComponentState(message.entityId, message.componentId, message.data, message.timestamp);
                return ProcessMessageResultType.StateUpdatedTimestamp;
            }

            // Outdated Message. Resend our state message through the wire.
            if (storedData.timestamp > message.timestamp)
            {
                return ProcessMessageResultType.StateOutdatedTimestamp;
            }

            int currentDataGreater = CompareData(storedData.data, message.data);

            // Same data, same timestamp. Weirdo echo message.
            if (currentDataGreater == 0)
            {
                return ProcessMessageResultType.NoChanges;
            }

            // Current data is greater
            if (currentDataGreater > 0)
            {
                return ProcessMessageResultType.StateOutdatedData;
            }

            // Curent data is lower
            UpdateSingleComponentState(message.entityId, message.componentId, message.data, message.timestamp);
            return ProcessMessageResultType.StateUpdatedData;
        }

        public EntityComponentData GetState(long entityId, int componentId)
        {
            TryGetSingleComponentState(entityId, componentId, out EntityComponentData entityComponentData);
            return entityComponentData;
        }

        public bool TryGetSingleComponentState(long entityId, int componentId, out EntityComponentData entityComponentData)
        {
            if (state.singleComponents.TryGetValue(componentId, out Dictionary<long, EntityComponentData> innerDictionary))
            {
                if (innerDictionary.TryGetValue(entityId, out entityComponentData))
                {
                    return true;
                }
            }
            entityComponentData = null;
            return false;
        }


        public bool TryGetComponentSetState(long entityId, int componentId, out List<EntityComponentData> entityComponentSet)
        {
            if (state.setComponents.TryGetValue(componentId, out Dictionary<long, List<EntityComponentData>> innerDictionary))
            {
                if (innerDictionary.TryGetValue(entityId, out entityComponentSet))
                {
                    return true;
                }
            }
            entityComponentSet = null;
            return false;
        }


        internal CrdtState GetState()
        {
            return state;
        }


        public List<CRDTMessage> GetStateAsMessages()
        {
            List<CRDTMessage> crdtMessagesList = new List<CRDTMessage>();

            foreach (var component in state.singleComponents)
            {
                foreach (var entityComponentData in component.Value)
                {
                    crdtMessagesList.Add(new CRDTMessage(){
                        type = entityComponentData.Value.data == null ? CrdtMessageType.DELETE_COMPONENT : CrdtMessageType.PUT_COMPONENT,
                        entityId = entityComponentData.Key,
                        componentId = component.Key,
                        timestamp = entityComponentData.Value.timestamp,
                        data = entityComponentData.Value.data
                    });
                }
            }


            foreach (var component in state.setComponents)
            {
                foreach (var set in component.Value)
                {
                    foreach (var entityComponentData in set.Value)
                    {
                        crdtMessagesList.Add(new CRDTMessage()
                        {
                            type = CrdtMessageType.APPEND_COMPONENT,
                            entityId = set.Key,
                            componentId = component.Key,
                            timestamp = entityComponentData.timestamp,
                            data = entityComponentData.data
                        });
                    }
                }
            }

            foreach (var entity in state.deletedEntitiesSet)
            {
                long entityNumber = entity.Key;
                long entityVersion = entity.Value;
                long entityId = entityNumber | (entityVersion << 16);
                crdtMessagesList.Add(new CRDTMessage(){
                    type = CrdtMessageType.DELETE_ENTITY,
                    entityId = entityId
                });
            }


            return crdtMessagesList;
        }

        public CRDTMessage CreateLwwMessage(int entityId, int componentId, byte[] data)
        {
            var result = new CRDTMessage()
            {
                entityId = entityId,
                componentId = componentId,
                data = data,
                timestamp = 0,
                type = data == null ? CrdtMessageType.DELETE_COMPONENT : CrdtMessageType.PUT_COMPONENT,
            };
            if (TryGetSingleComponentState(result.entityId, result.componentId, out EntityComponentData storedMessage))
            {
                result.timestamp = storedMessage.timestamp + 1;
            }
            return result;
        }

        public CRDTMessage CreateSetMessage(int entityId, int componentId, byte[] data)
        {
            var result = new CRDTMessage()
            {
                entityId = entityId,
                componentId = componentId,
                data = data,
                timestamp = 0,
                type = CrdtMessageType.APPEND_COMPONENT
            };
            if (TryGetSingleComponentState(result.entityId, result.componentId, out EntityComponentData storedMessage))
            {
                result.timestamp = storedMessage.timestamp + 1;
            }
            return result;
        }

        public void Clear()
        {
            state.singleComponents.Clear();
            state.deletedEntitiesSet.Clear();
            clearOnUpdated = false;
        }

        public void ClearOnUpdated()
        {
            clearOnUpdated = true;
        }

        private void UpdateSingleComponentState(long entityId, int componentId, object data, int remoteTimestamp)
        {
            bool stateExists = TryGetSingleComponentState(entityId, componentId, out EntityComponentData currentStateValue);
            if (stateExists)
            {
                currentStateValue.data = data;
                currentStateValue.timestamp = Math.Max(remoteTimestamp, currentStateValue.timestamp);
            }
            else
            {
                EntityComponentData newState = new EntityComponentData()
                {
                    timestamp = remoteTimestamp,
                    data = data
                };

                state.singleComponents.TryGetValue(componentId, out Dictionary<long, EntityComponentData> componentSet);
                if (componentSet != null)
                {
                    componentSet.Add(entityId, newState);
                }
                else
                {
                    state.singleComponents.Add(componentId, new Dictionary<long, EntityComponentData>());
                    state.singleComponents[componentId].Add(entityId, newState);
                }
            }
        }


        /**
         * @returns true if the element is added or false if it already exists
         */
        private bool TryAddSetComponentState(long entityId, int componentId, object data, int remoteTimestamp)
        {
            bool stateExists = TryGetComponentSetState(entityId, componentId, out List<EntityComponentData> currentSetState);

            EntityComponentData newState = new EntityComponentData()
            {
                timestamp = remoteTimestamp,
                data = data
            };

            // The entity already has a Set
            if (stateExists)
            {
                int index = currentSetState.BinarySearch(newState);

                if (index < 0)
                {
                    index = ~index;
                    currentSetState.Insert(index, newState);

                    if (currentSetState.Count > MAX_ELEMENT_SET)
                    {
                        currentSetState.RemoveRange(MAX_ELEMENT_SET, currentSetState.Count - MAX_ELEMENT_SET);
                    }
                }
                else
                {
                    // If the element already exist, we don't add it twice.
                    return false;
                }
            }
            else
            {
                state.setComponents.TryGetValue(componentId, out Dictionary<long, List<EntityComponentData>> componentSet);

                // The component is already in the dictionary, we have to create the new List for the Entity
                if (componentSet != null)
                {
                    componentSet.Add(entityId, new List<EntityComponentData>() { newState });
                }
                else
                {
                    state.setComponents.Add(componentId, new Dictionary<long, List<EntityComponentData>>());
                    state.setComponents[componentId].Add(entityId, new List<EntityComponentData>() { newState });
                }
            }

            return true;
        }

        /**
         * Compare raw data.
         * @internal
         * @returns 0 if is the same data, 1 if a > b, -1 if b > a
         */
        public static int CompareData(object a, object b)
        {
            // At reference level
            if (a == b) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            if (a is byte[] bytesA && b is byte[] bytesB)
            {
                int res;
                int n = bytesA.Length > bytesB.Length ? bytesB.Length : bytesA.Length;
                for (int i = 0; i < n; i++)
                {
                    res = bytesA[i] - bytesB[i];
                    if(res != 0)
                    {
                        return res > 0 ? 1 : -1;
                    }
                }

                res = bytesA.Length - bytesB.Length;
                return res > 0 ? 1 : res < 0 ? -1 : 0;
            }

            if (a is string strA && b is string strB)
            {
                return String.Compare(strA, strB, StringComparison.Ordinal);
            }

            //
            return a.Equals(b) ? 0 : -1;
        }

    }
}
