using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;
using System;
using Object = UnityEngine.Object;

namespace DCL.Components
{
    public class UIContainerStack : UIShape<UIContainerRectReferencesContainer, UIContainerStack.Model>
    {
        [System.Serializable]
        public new class Model : UIShape.Model
        {
            public Color color = Color.clear;
            public StackOrientation stackOrientation = StackOrientation.VERTICAL;
            public bool adaptWidth = true;
            public bool adaptHeight = true;
            public float spacing = 0;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.UiContainerStack)
                    return Utils.SafeUnimplemented<UIContainerStack, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UiContainerStack, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.UiContainerStack.HasName) pb.name = pbModel.UiContainerStack.Name;
                if (pbModel.UiContainerStack.HasParentComponent) pb.parentComponent = pbModel.UiContainerStack.ParentComponent;
                if (pbModel.UiContainerStack.HasVisible) pb.visible = pbModel.UiContainerStack.Visible;
                if (pbModel.UiContainerStack.HasOpacity) pb.opacity = pbModel.UiContainerStack.Opacity;
                if (pbModel.UiContainerStack.HasHAlign) pb.hAlign = pbModel.UiContainerStack.HAlign;
                if (pbModel.UiContainerStack.HasVAlign) pb.vAlign = pbModel.UiContainerStack.VAlign;
                if (pbModel.UiContainerStack.Width != null) pb.width = SDK6DataMapExtensions.FromProtobuf(pb.width, pbModel.UiContainerStack.Width);
                if (pbModel.UiContainerStack.Height != null) pb.height = SDK6DataMapExtensions.FromProtobuf(pb.height, pbModel.UiContainerStack.Height);
                if (pbModel.UiContainerStack.PositionX != null) pb.positionX = SDK6DataMapExtensions.FromProtobuf(pb.positionX, pbModel.UiContainerStack.PositionX);
                if (pbModel.UiContainerStack.PositionY != null) pb.positionY = SDK6DataMapExtensions.FromProtobuf(pb.positionY, pbModel.UiContainerStack.PositionY);
                if (pbModel.UiContainerStack.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.UiContainerStack.IsPointerBlocker;

                if (pbModel.UiContainerStack.HasStackOrientation) pb.stackOrientation = (StackOrientation)pbModel.UiContainerStack.StackOrientation;
                if (pbModel.UiContainerStack.HasAdaptWidth) pb.adaptWidth = pbModel.UiContainerStack.AdaptWidth;
                if (pbModel.UiContainerStack.HasAdaptHeight) pb.adaptHeight = pbModel.UiContainerStack.AdaptHeight;
                if (pbModel.UiContainerStack.HasSpacing) pb.spacing = pbModel.UiContainerStack.Spacing;
                if (pbModel.UiContainerStack.Color != null) pb.color = pbModel.UiContainerStack.Color.AsUnityColor();

                return pb;
            }
        }

        public enum StackOrientation
        {
            VERTICAL,
            HORIZONTAL
        }

        public Dictionary<string, GameObject> stackContainers = new ();

        HorizontalOrVerticalLayoutGroup layoutGroup;

        public UIContainerStack(UIShapePool pool) : base(pool)
        {
            this.pool = pool;
            model = new Model();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.UI_CONTAINER_STACK;

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null) =>
            Debug.LogError("Aborted UIContainerStack attachment to an entity. UIShapes shouldn't be attached to entities.");

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null) { }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (referencesContainer && referencesContainer.image)
            {
                referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);
                referencesContainer.image.raycastTarget = model.color.a >= RAYCAST_ALPHA_THRESHOLD;
            }

            if (childHookRectTransform)
            {
                switch (model.stackOrientation)
                {
                    case StackOrientation.VERTICAL when layoutGroup is not VerticalLayoutGroup:
                        Object.DestroyImmediate(layoutGroup, false);
                        layoutGroup = childHookRectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                        break;
                    case StackOrientation.HORIZONTAL when layoutGroup is not HorizontalLayoutGroup:
                        Object.DestroyImmediate(layoutGroup, false);
                        layoutGroup = childHookRectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
                        break;
                }
            }

            if (layoutGroup)
            {
                layoutGroup.childControlHeight = false;
                layoutGroup.childControlWidth = false;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.spacing = model.spacing;
            }

            if (referencesContainer)
            {
                referencesContainer.sizeFitter.adjustHeight = model.adaptHeight;
                referencesContainer.sizeFitter.adjustWidth = model.adaptWidth;
            }

            MarkLayoutDirty();
            return null;
        }

        private void RefreshContainerForShape(BaseDisposable updatedComponent)
        {
            UIShape childComponent = updatedComponent as UIShape;
            Assert.IsTrue(childComponent != null, "This should never happen!!!!");

            if (((UIShape.Model)childComponent.GetModel()).parentComponent != this.id)
            {
                MarkLayoutDirty();
                return;
            }

            if (!stackContainers.ContainsKey(childComponent.id))
            {
                var stackContainer = Object.Instantiate(
                    Resources.Load("UIContainerStackChild"), referencesContainer.childHookRectTransform, false) as GameObject;

#if UNITY_EDITOR
                stackContainer.name = "UIContainerStackChild - " + childComponent.id;
#endif
                stackContainers.Add(childComponent.id, stackContainer);

                int oldSiblingIndex = childComponent.referencesContainer.transform.GetSiblingIndex();
                childComponent.referencesContainer.transform.SetParent(stackContainer.transform, false);
                stackContainer.transform.SetSiblingIndex(oldSiblingIndex);
            }

            MarkLayoutDirty();
        }

        public override void OnChildAttached(UIShape parentComponent, UIShape childComponent)
        {
            RefreshContainerForShape(childComponent);
            childComponent.OnAppliedChanges -= RefreshContainerForShape;
            childComponent.OnAppliedChanges += RefreshContainerForShape;
        }

        protected override void RefreshDCLLayoutRecursively(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            base.RefreshDCLLayoutRecursively(refreshSize, refreshAlignmentAndPosition);
            referencesContainer.sizeFitter.RefreshRecursively();
        }

        protected override void OnChildDetached(UIShape parentComponent, UIShape childComponent)
        {
            if (parentComponent != this)
            {
                return;
            }

            if (stackContainers.ContainsKey(childComponent.id))
            {
                Object.Destroy(stackContainers[childComponent.id]);
                stackContainers[childComponent.id].transform.SetParent(null);
                stackContainers[childComponent.id].name += "- Detached";
                stackContainers.Remove(childComponent.id);
            }

            childComponent.OnAppliedChanges -= RefreshContainerForShape;
            RefreshDCLLayout();
        }
    }
}
