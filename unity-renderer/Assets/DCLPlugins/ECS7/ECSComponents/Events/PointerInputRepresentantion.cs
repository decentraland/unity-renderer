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

        // This represents the model component, since we have several components model, we just use their data
        private bool showFeedback = false;
        private string button;
        private string hoverText;
        private float distance;
        private string identifier;

        public PointerInputRepresentantion(PointerInputEventType type, IECSComponentWriter componentWriter)
        {
            this.type = type;
            this.componentWriter = componentWriter;
            pointerEventHandler = new OnPointerEventHandler();
        }

        public void SetData(IParcelScene scene, IDCLEntity entity, bool showFeedback, string button, float distance, string identifier, string hoverText)
        {
            this.scene = scene;
            eventEntity = entity;
            pointerEventHandler.SetColliders(entity);

            this.showFeedback = showFeedback;
            this.button = button;
            this.distance = distance;
            this.identifier = identifier;
            this.hoverText = hoverText;
        }

        public void Dispose()
        {
            pointerEventHandler.Dispose();
        }

        public Transform GetTransform() { return eventEntity.gameObject.transform; }
        
        public IDCLEntity entity => eventEntity;
        
        public void SetHoverState(bool hoverState)
        {
            pointerEventHandler.SetFeedbackState(showFeedback, hoverState, button, hoverText);
        }
        
        public bool IsAtHoverDistance(float distance) => distance <= this.distance;
        
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

                PBOnPointerResult result = CommonUtils.GetPointerResultModel(buttonId.ToString(), identifier, entityId, meshName, ray, hit);
                componentWriter.PutComponent(scene.sceneData.id, entityId, ComponentID.ON_POINTER_RESULT,
                    result);
            }
        }
        
        public PointerInputEventType GetEventType() {  return type; }
        
        public WebInterface.ACTION_BUTTON GetActionButton()
        {
            switch (button)
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

        public bool ShouldShowHoverFeedback()  => showFeedback;
        
        private bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsVisible() &&
                   IsAtHoverDistance(hit.distance) &&
                   (button == "ANY" || buttonId.ToString() == button);
        }
    }
}