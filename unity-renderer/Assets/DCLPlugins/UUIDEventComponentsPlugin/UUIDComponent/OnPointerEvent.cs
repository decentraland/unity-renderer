using System;
using System.Collections;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;
using Ray = UnityEngine.Ray;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class OnPointerEventHandler : IDisposable
    {
        public static bool enableInteractionHoverFeedback = true;
        public OnPointerEventColliders eventColliders { get; private set; }

        private IDCLEntity entity;

        public OnPointerEventHandler()
        {
            eventColliders = new OnPointerEventColliders();
        }

        public void SetColliders(IDCLEntity entity)
        {
            this.entity = entity;
            eventColliders.Initialize(entity);
        }

        public void UpdateCollidersEnabledBasedOnRenderers(IDCLEntity entity)
        {
            this.entity = entity;
            eventColliders.UpdateCollidersEnabledBasedOnRenderers(entity);
        }

        public void SetFeedbackState(bool showFeedback, bool hoverState, string button, string hoverText)
        {
            if (!enableInteractionHoverFeedback)
                return;

            var cursorData = DataStore.i.Get<DataStore_Cursor>();
            cursorData.hoverFeedbackEnabled.Set(showFeedback);

            if (showFeedback)
            {
                if (hoverState)
                {
                    cursorData.hoverFeedbackButton.Set(button);
                    cursorData.hoverFeedbackText.Set(hoverText);
                }

                cursorData.hoverFeedbackHoverState.Set(hoverState);
            }
        }

        public string GetMeshName(Collider collider)
        {
            if (collider == null || eventColliders == null)
                return null;

            return eventColliders.GetMeshName(collider);
        }

        public void Dispose()
        {
            eventColliders.Dispose();
        }
    }

    public class OnPointerEvent : UUIDComponent, IPointerInputEvent, IOutOfSceneBoundariesHandler
    {
        public static bool enableInteractionHoverFeedback = true;

        [Serializable]
        public new class Model : UUIDComponent.Model
        {
            public string button = WebInterface.ACTION_BUTTON.ANY.ToString();
            public string hoverText = "Interact";
            public float distance = 10f;
            public bool showFeedback = true;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.UuidCallback)
                    return Utils.SafeUnimplemented<OnPointerEvent, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UuidCallback, actual: pbModel.PayloadCase);

                var pb = new Model();

                if (pbModel.UuidCallback.HasUuid) pb.uuid = pbModel.UuidCallback.Uuid;
                if (pbModel.UuidCallback.HasType) pb.type = pbModel.UuidCallback.Type;
                if (pbModel.UuidCallback.HasButton) pb.button = pbModel.UuidCallback.Button;
                if (pbModel.UuidCallback.HasHoverText) pb.hoverText = pbModel.UuidCallback.HoverText;
                if (pbModel.UuidCallback.HasDistance) pb.distance = pbModel.UuidCallback.Distance;
                if (pbModel.UuidCallback.HasShowFeedback) pb.showFeedback = pbModel.UuidCallback.ShowFeedback;

                return pb;
            }

            public WebInterface.ACTION_BUTTON GetActionButton() =>
                button switch
                {
                    "PRIMARY" => WebInterface.ACTION_BUTTON.PRIMARY,
                    "SECONDARY" => WebInterface.ACTION_BUTTON.SECONDARY,
                    "POINTER" => WebInterface.ACTION_BUTTON.POINTER,
                    _ => WebInterface.ACTION_BUTTON.ANY,
                };
        }

        public OnPointerEventHandler pointerEventHandler;

        public override void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            base.Initialize(scene, entity);

            if (model == null)
                model = new OnPointerEvent.Model();

            pointerEventHandler = new OnPointerEventHandler();
            SetEventColliders(entity);

            entity.OnShapeUpdated -= SetEventColliders;
            entity.OnShapeUpdated += SetEventColliders;

            DataStore.i.sceneBoundariesChecker.Add(entity,this);
        }

        public WebInterface.ACTION_BUTTON GetActionButton()
        {
            return ((Model) this.model).GetActionButton();
        }

        public void SetHoverState(bool hoverState)
        {
            Model model = (Model) this.model;
            pointerEventHandler.SetFeedbackState(model.showFeedback, hoverState, model.button, model.hoverText);
        }

        void SetEventColliders(IDCLEntity entity)
        {
            pointerEventHandler.SetColliders(entity);
        }

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

        public bool IsAtHoverDistance(float distance)
        {
            Model model = this.model as Model;
            return distance <= model.distance;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            this.model = newModel ?? new Model();
            return null;
        }

        public bool ShouldShowHoverFeedback()
        {
            Model model = this.model as Model;
            return model.showFeedback;
        }

        void OnDestroy()
        {
            if (entity != null)
                entity.OnShapeUpdated -= SetEventColliders;

            DataStore.i.sceneBoundariesChecker.Remove(entity,this);

            pointerEventHandler.Dispose();
        }

        public virtual void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
        }

        public virtual PointerInputEventType GetEventType()
        {
            return PointerInputEventType.NONE;
        }

        public void UpdateOutOfBoundariesState(bool enable)
        {
            pointerEventHandler.UpdateCollidersEnabledBasedOnRenderers(entity);
        }
    }
}
