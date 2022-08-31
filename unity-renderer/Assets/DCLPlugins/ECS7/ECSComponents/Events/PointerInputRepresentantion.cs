using System;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;
using Ray = UnityEngine.Ray;

namespace DCLPlugins.ECSComponents.Events
{
    public class PointerInputRepresentantion : IPointerInputEvent
    {
        internal static int lamportTimestamp = 0;
        internal IDCLEntity eventEntity;
        private IParcelScene scene;
        private readonly IOnPointerEventHandler pointerEventHandler;
        internal readonly PointerEventType pointerEventType;
        private readonly PointerInputEventType inputEventType;
        private readonly IECSComponentWriter componentWriter;
        private readonly DataStore_ECS7 dataStore;

        // This represents the model component, since we have several components model, we just use their data
        internal bool showFeedback = false;
        internal ActionButton button;
        internal string hoverText;
        internal float distance;
        internal Queue<PointerEvent> pendingResolvingPointerEvents;

        public PointerInputRepresentantion(IDCLEntity entity, DataStore_ECS7 dataStore, PointerEventType pointerEventType, IECSComponentWriter componentWriter, IOnPointerEventHandler pointerEventHandler, Queue<PointerEvent> pendingResolvingPointerEvents)
        {
            this.dataStore = dataStore;
            this.pointerEventType = pointerEventType;
            this.componentWriter = componentWriter;
            this.pointerEventHandler = pointerEventHandler;
            this.pendingResolvingPointerEvents = pendingResolvingPointerEvents;

            // We set the equivalent enum of the proto 
            switch (pointerEventType)
            {
                case PointerEventType.Up:
                    inputEventType = PointerInputEventType.UP;
                    break;
                case PointerEventType.Down:
                    inputEventType = PointerInputEventType.DOWN;
                    break;
                default:
                    inputEventType = PointerInputEventType.NONE;
                    break;
            }
            
            Initializate(entity);
        }

        public void SetData(IParcelScene scene, IDCLEntity entity, bool showFeedback, ActionButton button, float distance, string hoverText)
        {
            this.scene = scene;
            eventEntity = entity;

            this.showFeedback = showFeedback;
            this.button = button;
            this.distance = distance;
            this.hoverText = hoverText;
        }

        public void Dispose()
        {
            dataStore.shapesReady.OnAdded -= ConfigureColliders;
            pointerEventHandler.Dispose();
        }

        public Transform GetTransform() { return eventEntity.gameObject.transform; }
        
        IDCLEntity IPointerEvent.entity => eventEntity;
        
        void IPointerEvent.SetHoverState(bool hoverState)
        {
            pointerEventHandler.SetFeedbackState(showFeedback, hoverState, button.ToString(), hoverText);
        }
        
        public bool /*IPointerEvent*/ IsAtHoverDistance(float distance) => distance <= this.distance;
        
        public bool /*IPointerEvent*/ IsVisible()
        {
            if (eventEntity == null)
                return false;

            bool isVisible = false;

            if (eventEntity.meshesInfo != null &&
                eventEntity.meshesInfo.renderers != null &&
                eventEntity.meshesInfo.renderers.Length > 0)
            {
                isVisible = eventEntity.meshesInfo.renderers[0].enabled;
            }

            return isVisible;
        }
        
        void IPointerInputEvent.Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!IsVisible())
                return;

            if (ShouldReportEvent(buttonId, hit))
                ReportEvent(buttonId, ray, hit);
        }

        internal void ReportEvent(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            string meshName = pointerEventHandler.GetMeshName(hit.collider);
            long entityId = eventEntity.entityId;

            PointerEvent pointerEvent = new PointerEvent();
            pointerEvent.sceneId = scene.sceneData.id;
            pointerEvent.button = (ActionButton)buttonId;
            pointerEvent.hit = ProtoConvertUtils.ToPBRaycasHit(entityId, meshName, ray, hit);
            pointerEvent.type = pointerEventType;
            pointerEvent.timestamp = GetLamportTimestamp();
                
            pendingResolvingPointerEvents.Enqueue(pointerEvent);
        }
        
        public /*IPointerInputEvent*/ PointerInputEventType GetEventType() => inputEventType; 
        
        WebInterface.ACTION_BUTTON IPointerInputEvent.GetActionButton() => (WebInterface.ACTION_BUTTON) button;

        bool IPointerInputEvent.ShouldShowHoverFeedback()  => showFeedback;
        
        private void ConfigureColliders(long entityId, GameObject shapeGameObject) => pointerEventHandler.SetColliders(eventEntity);
        
        private bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsVisible() &&
                   IsAtHoverDistance(hit.distance) &&
                   ((int)button == (int)WebInterface.ACTION_BUTTON.ANY || buttonId == (WebInterface.ACTION_BUTTON)button);
        }
        
        private void Initializate(IDCLEntity entity)
        {
            if(dataStore.shapesReady.ContainsKey(entity.entityId))
                pointerEventHandler.SetColliders(entity);

            dataStore.shapesReady.OnAdded += ConfigureColliders;
        }

        private int GetLamportTimestamp()
        {
            int result = lamportTimestamp;
            lamportTimestamp++;
            return result;
        }
    }
}