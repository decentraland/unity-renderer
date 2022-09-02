using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Events;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using PointerEvent = DCLPlugins.ECSComponents.Events.PointerEvent;

namespace DCLPlugins.ECSComponents.OnPointerDown
{
    public class PointerEventsComponentHandler : IECSComponentHandler<PBPointerEvents>
    {
        private IECSComponentWriter componentWriter;
        private DataStore_ECS7 dataStore;
        private IECSContext context;

        internal readonly Dictionary<PBPointerEventEntry, PointerInputRepresentation> pointerEventsDictionary = new Dictionary<PBPointerEventEntry, PointerInputRepresentation>();

        public PointerEventsComponentHandler(IECSComponentWriter componentWriter, DataStore_ECS7 dataStore, IECSContext context)
        {
            this.dataStore = dataStore;
            this.componentWriter = componentWriter;

            this.context = context;
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            // We dispose all PointerInputRepresentation
            using (var representations = pointerEventsDictionary.Values.GetEnumerator())
            {
                while (representations.MoveNext())
                {
                    DisposePointerInputRepresentantion(entity.entityId, representations.Current);
                }
            }
            
            pointerEventsDictionary.Clear();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBPointerEvents model)
        {
            // We copy all the currents pointer input representation into a new Dict, if they don't exist in the current model, we will remove then after the model has been processed 
            Dictionary<PBPointerEventEntry, PointerInputRepresentation> pointerInputRepresentationsToRemove = new Dictionary<PBPointerEventEntry, PointerInputRepresentation>(pointerEventsDictionary);
            
            // We process all the pointer events that has been received
            using (var pointerEvents = model.PointerEvents.GetEnumerator())
            {
                while (pointerEvents.MoveNext())
                {
                    var pointerEventEntry = pointerEvents.Current;
                    
                    // If it is the first time that we receive this pointer event, we create a representation for it and add it to the data store
                    if (!pointerEventsDictionary.TryGetValue(pointerEventEntry, out PointerInputRepresentation representantion))
                    {
                        representantion = new PointerInputRepresentation(entity, dataStore, pointerEventEntry.EventType, componentWriter, new OnPointerEventHandler(), context.systemsContext.pendingResolvingPointerEvents);
                        dataStore.AddPointerEvent(entity.entityId, representantion);
                        pointerEventsDictionary.Add(pointerEventEntry, representantion);
                    }

                    // We update the data of the representation 
                    representantion.SetData(scene, entity, pointerEventEntry.EventInfo.GetShowFeedback(), pointerEventEntry.EventInfo.GetButton(), pointerEventEntry.EventInfo.GetMaxDistance(), pointerEventEntry.EventInfo.GetHoverText());
                
                    // Since the pointer event still exists, we remove it from the removal list
                    pointerInputRepresentationsToRemove.Remove(pointerEventEntry);
                }
            }

            // If a pointer event has been removed from the last time that we checked the model, we must remove and dispose it
            if (pointerInputRepresentationsToRemove.Count != 0)
            {
                using var representationsToRemove = pointerInputRepresentationsToRemove.GetEnumerator();
                while (representationsToRemove.MoveNext())
                {
                    var kvp = representationsToRemove.Current;
                    DisposePointerInputRepresentantion(entity.entityId, kvp.Value);
                    pointerEventsDictionary.Remove(kvp.Key);
                }
            }
        }

        private void DisposePointerInputRepresentantion(long entityId, PointerInputRepresentation representation)
        {
            dataStore.RemovePointerEvent(entityId, representation);
            representation?.Dispose();
        }
    }
}