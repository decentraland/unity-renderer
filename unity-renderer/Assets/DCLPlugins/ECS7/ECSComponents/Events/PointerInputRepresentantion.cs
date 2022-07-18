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
        private static int lamportTimestamp = 0;
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

        public PointerInputRepresentantion(IDCLEntity entity, DataStore_ECS7 dataStore, PointerInputEventType type, IECSComponentWriter componentWriter)
        {
            this.dataStore = dataStore;
            this.type = type;
            this.componentWriter = componentWriter;
            pointerEventHandler = new OnPointerEventHandler();
            eventEntity = entity;

            Initializate(entity);
        }

        public void SetData(IParcelScene scene, bool showFeedback, int button, float distance, string hoverText)
        {
            this.scene = scene;

            this.showFeedback = showFeedback;
            this.button = button;
            this.distance = distance;
            this.hoverText = hoverText;
        }

        public void Dispose()
        {
            dataStore.shapesReady.OnAdded -= CreateColliders;
            pointerEventHandler.Dispose();
        }

        public Transform GetTransform() { return eventEntity.gameObject.transform; }

        IDCLEntity IPointerEvent.entity => eventEntity;

        void IPointerEvent.SetHoverState(bool hoverState) { pointerEventHandler.SetFeedbackState(showFeedback, hoverState, button.ToString(), hoverText); }

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
                int componentId;

                // We have to differentiate between OnPointerDown and OnPointerUp
                switch (type)
                {
                    case PointerInputEventType.DOWN:
                        componentId = ComponentID.ON_POINTER_DOWN_RESULT;

                        PBOnPointerDownResult downPayload = ProtoConvertUtils.GetPointerDownResultModel((int)buttonId, meshName, ray, hit);
                        downPayload.Timestamp = GetLamportTimestamp();
                        componentWriter.PutComponent(scene.sceneData.id, entityId, componentId,
                            downPayload);
                        break;
                    case PointerInputEventType.UP:
                        componentId = ComponentID.ON_POINTER_UP_RESULT;

                        PBOnPointerUpResult payload = ProtoConvertUtils.GetPointerUpResultModel((int)buttonId, meshName, ray, hit);
                        payload.Timestamp = GetLamportTimestamp();
                        componentWriter.PutComponent(scene.sceneData.id, entityId, componentId,
                            payload);
                        break;
                }
            }
        }

        PointerInputEventType IPointerInputEvent.GetEventType() => type;

        WebInterface.ACTION_BUTTON IPointerInputEvent.GetActionButton() => (WebInterface.ACTION_BUTTON) button;

        bool IPointerInputEvent.ShouldShowHoverFeedback()  => showFeedback;

        private void CreateColliders(long entityId, GameObject shapeGameObject)
        {
            if(entityId != eventEntity.entityId)
                return;
            
            pointerEventHandler.CreateColliders(eventEntity);
        }


        private bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsVisible() &&
                   IsAtHoverDistance(hit.distance) &&
                   (button == (int)WebInterface.ACTION_BUTTON.ANY || buttonId == (WebInterface.ACTION_BUTTON)button);
        }
        
        private void Initializate(IDCLEntity entity)
        {
            // If we already have created the collider in another OnPointerEvent, we can skip since we have 1 collider for all OnPointerEvent
            if (dataStore.entitiesOnPointerEvent.ContainsKey(entity.entityId))
                return;
            
            // If the shape is already ready, we create the colliders now, if not, we wait to the shape to be ready and then we create them
            if (dataStore.shapesReady.ContainsKey(entity.entityId))
                CreateColliders(entity.entityId, entity.meshRootGameObject);
            
            dataStore.shapesReady.OnAdded += CreateColliders;
        }

        private int GetLamportTimestamp()
        {
            int result = lamportTimestamp;
            lamportTimestamp++;
            return result;
        }
    }
}