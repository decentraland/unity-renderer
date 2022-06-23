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

namespace DCLPlugins.ECS7.ECSComponents.Events.OnPointerDown
{
    public class PointerInputRepresentantion : IPointerInputEvent
    {
        private IDCLEntity eventEntity;
        private IParcelScene scene;
        private PBOnPointerDown model;
        private readonly OnPointerEventHandler pointerEventHandler;
        private readonly PointerInputEventType type;
        private readonly IECSComponentWriter componentWriter;
        
        public PointerInputRepresentantion(PointerInputEventType type, IECSComponentWriter componentWriter)
        {
            this.type = type;
            this.componentWriter = componentWriter;
            pointerEventHandler = new OnPointerEventHandler();
        }

        public void SetData(IParcelScene scene, IDCLEntity entity, PBOnPointerDown onPointerDown)
        {
            this.scene = scene;
            eventEntity = entity;
            model = onPointerDown;
            pointerEventHandler.SetColliders(entity);
        }

        public void Dispose()
        {
            pointerEventHandler.Dispose();
        }

        public Transform GetTransform() { return eventEntity.gameObject.transform; }
        
        public IDCLEntity entity => eventEntity;
        
        public void SetHoverState(bool hoverState)
        {
            pointerEventHandler.SetFeedbackState(model.ShowFeedback, hoverState, model.Button, model.HoverText);
        }
        
        public bool IsAtHoverDistance(float distance) => distance <= model.Distance;
        
        public bool IsVisible()
        {
            if (entity == null)
                return false;

            bool isVisible = false;

            if (entity.meshesInfo != null &&
                entity.meshesInfo.renderers != null &&
                entity.meshesInfo.renderers.Length > 0)
            {
                isVisible = entity.meshesInfo.renderers[0].enabled;
            }

            return isVisible;
        }
        
        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!IsVisible())
                return;

            if (ShouldReportEvent(buttonId, hit))
            {
                string meshName = pointerEventHandler.GetMeshName(hit.collider);
                long entityId = entity.entityId;

                PBOnPointerResult result = CommonUtils.GetPointerResultModel(buttonId.ToString(), model.Identifier, entityId, meshName, ray, hit);
                componentWriter.PutComponent(scene.sceneData.id, entityId, ComponentID.ON_POINTER_RESULT,
                    result);
            }
        }
        
        public PointerInputEventType GetEventType() {  return type; }
        
        public WebInterface.ACTION_BUTTON GetActionButton()
        {
            switch (model.Button)
            {
                case "PRIMARY":
                    return WebInterface.ACTION_BUTTON.PRIMARY;
                case "SECONDARY":
                    return WebInterface.ACTION_BUTTON.SECONDARY;
                case "POINTER":
                    return WebInterface.ACTION_BUTTON.POINTER;
                default:
                    return WebInterface.ACTION_BUTTON.ANY;
            }
        }

        public bool ShouldShowHoverFeedback()  => model.ShowFeedback;
        
        private bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsVisible() &&
                   IsAtHoverDistance(hit.distance) &&
                   (model.Button == "ANY" || buttonId.ToString() == model.Button);
        }
    }
}