using System;
using System.Collections;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using UnityEngine;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public enum PointerInputEventType
    {
        NONE,
        CLICK,
        DOWN,
        UP
    }

    public interface IPointerEvent : IMonoBehaviour
    {
        IDCLEntity entity { get; }
        void SetHoverState(bool state);
        bool IsAtHoverDistance(float distance);
        bool IsVisible();
    }

    public interface IPointerInputEvent : IPointerEvent
    {
        void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit);
        PointerInputEventType GetEventType();
        WebInterface.ACTION_BUTTON GetActionButton();
        bool ShouldShowHoverFeedback();
    }

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

    public class OnPointerEvent : UUIDComponent, IPointerInputEvent
    {
        public static bool enableInteractionHoverFeedback = true;

        [System.Serializable]
        public new class Model : UUIDComponent.Model
        {
            public string button = WebInterface.ACTION_BUTTON.ANY.ToString();
            public string hoverText = "Interact";
            public float distance = 10f;
            public bool showFeedback = true;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

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

            pointerEventHandler.Dispose();
        }

        public virtual void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
        }

        public virtual PointerInputEventType GetEventType()
        {
            return PointerInputEventType.NONE;
        }
    }
}