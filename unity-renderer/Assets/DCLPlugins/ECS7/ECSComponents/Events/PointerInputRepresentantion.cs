using System;
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

namespace DCLPlugins.ECSComponents
{
    public class PointerInputRepresentantion : IPointerInputEvent
    {
        private IDCLEntity eventEntity;
        private IParcelScene scene;
        private readonly OnPointerEventHandler pointerEventHandler;
        private readonly PointerInputEventType type;
        private readonly IECSComponentWriter componentWriter;
        private readonly DataStore_ECS7 dataStore;

        // This represents the model component, since we have several components model, we just use their data
        private bool showFeedback = false;
        private int button;
        private string hoverText;
        private float distance;

        public PointerInputRepresentantion(IDCLEntity entity,DataStore_ECS7 dataStore,PointerInputEventType type, IECSComponentWriter componentWriter)
        {
            this.dataStore = dataStore;
            this.type = type;
            this.componentWriter = componentWriter;
            pointerEventHandler = new OnPointerEventHandler();
            
            Initializate(entity);
        }

        public void SetData(IParcelScene scene, IDCLEntity entity, bool showFeedback, int button, float distance, string hoverText)
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
            {
                string meshName = pointerEventHandler.GetMeshName(hit.collider);
                long entityId = eventEntity.entityId;

                object payload = null;
                int componentId = ComponentID.ON_POINTER_UP_RESULT;
                
                // We have to differentiate between OnPointerDown and OnPointerUp
                switch (type)
                {
                    case PointerInputEventType.DOWN:
                        componentId = ComponentID.ON_POINTER_DOWN_RESULT;
                        payload = ProtoConvertUtils.GetPointerDownResultModel((int)buttonId, meshName, ray, hit);
                        break;
                    case PointerInputEventType.UP:
                        componentId = ComponentID.ON_POINTER_UP_RESULT;
                        payload = ProtoConvertUtils.GetPointerUpResultModel((int)buttonId, meshName, ray, hit);
                        break;
                }
                
                // We set the result
                componentWriter.PutComponent(scene.sceneData.id, entityId, componentId,
                    payload);
            }
        }
        
        PointerInputEventType IPointerInputEvent.GetEventType() {  return type; }
        
        WebInterface.ACTION_BUTTON IPointerInputEvent.GetActionButton()
        {
            return (WebInterface.ACTION_BUTTON) button;
        }

        bool IPointerInputEvent.ShouldShowHoverFeedback()  => showFeedback;
        
        private bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsVisible() &&
                   IsAtHoverDistance(hit.distance) &&
                   (button == (int)WebInterface.ACTION_BUTTON.ANY || buttonId == (WebInterface.ACTION_BUTTON)button);
        }
        
        private void ConfigureColliders(long entityId, GameObject shapeGameObject)
        {
            pointerEventHandler.SetColliders(eventEntity);
        }
        
        private void Initializate(IDCLEntity entity)
        {
            if(dataStore.shapesReady.ContainsKey(entity.entityId))
                pointerEventHandler.SetColliders(entity);

            dataStore.shapesReady.OnAdded += ConfigureColliders;
        }
    }
}