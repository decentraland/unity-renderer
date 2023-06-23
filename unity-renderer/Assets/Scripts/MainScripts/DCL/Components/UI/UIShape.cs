using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using DCL.Components.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;
using Unity.Profiling;

namespace DCL.Components
{
    public class UIShape : BaseDisposable, IUIRefreshable
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string name;
            public string parentComponent;
            public bool visible = true;
            public float opacity = 1f;
            public string hAlign = "center";
            public string vAlign = "center";
            public UIValue width = new (100f);
            public UIValue height = new (50f);
            public UIValue positionX = new (0f);
            public UIValue positionY = new (0f);
            public bool isPointerBlocker = true;
            public string onClick;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {

                switch (pbModel.PayloadCase)
                {
                    case ComponentBodyPayload.PayloadOneofCase.UiShape:
                        var uiShapeModel = new Model();
                        if (pbModel.UiShape.HasName) uiShapeModel.name = pbModel.UiShape.Name;
                        if (pbModel.UiShape.HasParentComponent) uiShapeModel.parentComponent = pbModel.UiShape.ParentComponent;
                        if (pbModel.UiShape.HasVisible) uiShapeModel.visible = pbModel.UiShape.Visible;
                        if (pbModel.UiShape.HasOpacity) uiShapeModel.opacity = pbModel.UiShape.Opacity;
                        if (pbModel.UiShape.HasHAlign) uiShapeModel.hAlign = pbModel.UiShape.HAlign;
                        if (pbModel.UiShape.HasVAlign) uiShapeModel.vAlign = pbModel.UiShape.VAlign;
                        if (pbModel.UiShape.Width != null) uiShapeModel.width = SDK6DataMapExtensions.FromProtobuf(uiShapeModel.width, pbModel.UiShape.Width);
                        if (pbModel.UiShape.Height != null) uiShapeModel.height = SDK6DataMapExtensions.FromProtobuf(uiShapeModel.height, pbModel.UiShape.Height);
                        if (pbModel.UiShape.PositionX != null) uiShapeModel.positionX = SDK6DataMapExtensions.FromProtobuf(uiShapeModel.positionX, pbModel.UiShape.PositionX);
                        if (pbModel.UiShape.PositionY != null) uiShapeModel.positionY = SDK6DataMapExtensions.FromProtobuf(uiShapeModel.positionY, pbModel.UiShape.PositionY);
                        if (pbModel.UiShape.HasIsPointerBlocker) uiShapeModel.isPointerBlocker = pbModel.UiShape.IsPointerBlocker;
                        return uiShapeModel;

                    case ComponentBodyPayload.PayloadOneofCase.UiScreenSpaceShape:
                        var screenSpaceModel = new Model();
                        if (pbModel.UiScreenSpaceShape.HasName) screenSpaceModel.name = pbModel.UiScreenSpaceShape.Name;
                        if (pbModel.UiScreenSpaceShape.HasParentComponent) screenSpaceModel.parentComponent = pbModel.UiScreenSpaceShape.ParentComponent;
                        if (pbModel.UiScreenSpaceShape.HasVisible) screenSpaceModel.visible = pbModel.UiScreenSpaceShape.Visible;
                        if (pbModel.UiScreenSpaceShape.HasOpacity) screenSpaceModel.opacity = pbModel.UiScreenSpaceShape.Opacity;
                        if (pbModel.UiScreenSpaceShape.HasHAlign) screenSpaceModel.hAlign = pbModel.UiScreenSpaceShape.HAlign;
                        if (pbModel.UiScreenSpaceShape.HasVAlign) screenSpaceModel.vAlign = pbModel.UiScreenSpaceShape.VAlign;
                        if (pbModel.UiScreenSpaceShape.Width != null) screenSpaceModel.width = SDK6DataMapExtensions.FromProtobuf(screenSpaceModel.width, pbModel.UiScreenSpaceShape.Width);
                        if (pbModel.UiScreenSpaceShape.Height != null) screenSpaceModel.height = SDK6DataMapExtensions.FromProtobuf(screenSpaceModel.height, pbModel.UiScreenSpaceShape.Height);
                        if (pbModel.UiScreenSpaceShape.PositionX != null) screenSpaceModel.positionX = SDK6DataMapExtensions.FromProtobuf(screenSpaceModel.positionX, pbModel.UiScreenSpaceShape.PositionX);
                        if (pbModel.UiScreenSpaceShape.PositionY != null) screenSpaceModel.positionY = SDK6DataMapExtensions.FromProtobuf(screenSpaceModel.positionY, pbModel.UiScreenSpaceShape.PositionY);
                        if (pbModel.UiScreenSpaceShape.HasIsPointerBlocker) screenSpaceModel.isPointerBlocker = pbModel.UiScreenSpaceShape.IsPointerBlocker;
                        return screenSpaceModel;

                    case ComponentBodyPayload.PayloadOneofCase.UiFullScreenShape:
                        var fullScreenModel = new Model();
                        if (pbModel.UiFullScreenShape.HasName) fullScreenModel.name = pbModel.UiFullScreenShape.Name;
                        if (pbModel.UiFullScreenShape.HasParentComponent) fullScreenModel.parentComponent = pbModel.UiFullScreenShape.ParentComponent;
                        if (pbModel.UiFullScreenShape.HasVisible) fullScreenModel.visible = pbModel.UiFullScreenShape.Visible;
                        if (pbModel.UiFullScreenShape.HasOpacity) fullScreenModel.opacity = pbModel.UiFullScreenShape.Opacity;
                        if (pbModel.UiFullScreenShape.HasHAlign) fullScreenModel.hAlign = pbModel.UiFullScreenShape.HAlign;
                        if (pbModel.UiFullScreenShape.HasVAlign) fullScreenModel.vAlign = pbModel.UiFullScreenShape.VAlign;
                        if (pbModel.UiFullScreenShape.Width != null) fullScreenModel.width = SDK6DataMapExtensions.FromProtobuf(fullScreenModel.width, pbModel.UiFullScreenShape.Width);
                        if (pbModel.UiFullScreenShape.Height != null) fullScreenModel.height = SDK6DataMapExtensions.FromProtobuf(fullScreenModel.height, pbModel.UiFullScreenShape.Height);
                        if (pbModel.UiFullScreenShape.PositionX != null) fullScreenModel.positionX = SDK6DataMapExtensions.FromProtobuf(fullScreenModel.positionX, pbModel.UiFullScreenShape.PositionX);
                        if (pbModel.UiFullScreenShape.PositionY != null) fullScreenModel.positionY = SDK6DataMapExtensions.FromProtobuf(fullScreenModel.positionY, pbModel.UiFullScreenShape.PositionY);
                        if (pbModel.UiFullScreenShape.HasIsPointerBlocker) fullScreenModel.isPointerBlocker = pbModel.UiFullScreenShape.IsPointerBlocker;
                        return fullScreenModel;

                    default:
                        return Utils.SafeUnimplemented<UIShape, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UiShape, actual: pbModel.PayloadCase);
                }
            }
        }

        public override string componentName => GetDebugName();
        protected virtual string referencesContainerPrefabName => "";
        public UIReferencesContainer referencesContainer;
        public RectTransform childHookRectTransform;

        public bool isLayoutDirty { get; private set; }

        protected UIShapePool pool;
        private BaseVariable<Vector2Int> screenSize => DataStore.i.screen.size;
        private BaseVariable<Dictionary<int, Queue<IUIRefreshable>>> dirtyShapesBySceneVariable => DataStore.i.HUDs.dirtyShapes;
        protected UIShape parentUIComponent { get; private set; }

        private System.Action OnLayoutRefresh;

        public UIShape(UIShapePool pool)
        {
            this.pool = pool;
            screenSize.OnChange += OnScreenResize;
            model = new Model();
        }

        private void OnScreenResize(Vector2Int current, Vector2Int previous)
        {
            if (GetRootParent() == this)
                RequestRefresh();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.UI_IMAGE_SHAPE;

        private string GetDebugName()
        {
            Model model = (Model) this.model;

            return string.IsNullOrEmpty(model.name)
                ? GetType().Name
                : GetType().Name + " - " + model.name;
        }

        public override IEnumerator ApplyChanges(BaseModel newJson) =>
            null;

        ProfilerMarker m_UIShapeInstantiateUIGameObject = new ("VV.UIShape.InstantiateUIGameObject");

        internal T InstantiateUIGameObject<T>(string prefabPath) where T : UIReferencesContainer
        {
            m_UIShapeInstantiateUIGameObject.Begin();
            Model model = (Model) this.model;

            bool targetParentExists = !string.IsNullOrEmpty(model.parentComponent) &&
                                      scene.componentsManagerLegacy.HasSceneSharedComponent(model.parentComponent);

            if (targetParentExists)
            {
                if (scene.componentsManagerLegacy.HasSceneSharedComponent(model.parentComponent))
                {
                    parentUIComponent = (scene.componentsManagerLegacy.GetSceneSharedComponent(model.parentComponent) as UIShape);
                }
                else
                {
                    parentUIComponent = scene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();
                }
            }
            else
            {
                parentUIComponent = scene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();
            }

            // var uiGameObject = Object.Instantiate(Resources.Load(prefabPath), parentUIComponent?.childHookRectTransform) as GameObject;
            referencesContainer = pool.TakeUIShape();// uiGameObject.GetComponent<T>();

            if (parentUIComponent != null)
                referencesContainer.transform.parent = parentUIComponent?.childHookRectTransform;

            referencesContainer.rectTransform.SetToMaxStretch();

            childHookRectTransform = referencesContainer.childHookRectTransform;

            referencesContainer.owner = this;

            m_UIShapeInstantiateUIGameObject.End();
            return referencesContainer as T;
        }

        private void RequestRefresh()
        {
            if (isLayoutDirty) return;

            isLayoutDirty = true;

            var dirtyShapesByScene = dirtyShapesBySceneVariable.Get();

            int sceneDataSceneNumber = scene.sceneData.sceneNumber;
            if (sceneDataSceneNumber <= 0) sceneDataSceneNumber = 666;

            if (!dirtyShapesByScene.ContainsKey(sceneDataSceneNumber))
            {
                dirtyShapesByScene.Add(sceneDataSceneNumber, new Queue<IUIRefreshable>());
            }

            dirtyShapesByScene[sceneDataSceneNumber].Enqueue(this);
        }

        private void RefreshRecursively()
        {
            // We are not using the _Internal here because the method is overridden
            // by some UI shapes.
            RefreshDCLLayoutRecursively(refreshSize: true, refreshAlignmentAndPosition: false);
            FixMaxStretchRecursively();
            RefreshDCLLayoutRecursively_Internal(refreshSize: false, refreshAlignmentAndPosition: true);
        }

        protected void MarkLayoutDirty( System.Action OnRefresh = null )
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            if (rootParent.referencesContainer == null)
                return;

            rootParent.RequestRefresh();

            if ( OnRefresh != null )
                rootParent.OnLayoutRefresh += OnRefresh;
        }

        ProfilerMarker m_UIShapeRefreshDCLLayout = new ("VV.UIShape.RefreshDCLLayout");
        ProfilerMarker m_UIShapeRefreshDCLSize = new ("VV.UIShape.RefreshDCLSize");
        ProfilerMarker m_UIShapeRefreshDCLAlignmentAndPosition = new ("VV.UIShape.RefreshDCLAlignmentAndPosition");

        protected void RefreshDCLLayout(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            m_UIShapeRefreshDCLLayout.Begin();
            RectTransform parentRT = referencesContainer.GetComponentInParent<RectTransform>();

            if (refreshSize)
            {
                m_UIShapeRefreshDCLSize.Begin();
                RefreshDCLSize(parentRT);
                m_UIShapeRefreshDCLSize.End();
            }

            if (refreshAlignmentAndPosition)
            {
                m_UIShapeRefreshDCLAlignmentAndPosition.Begin();
                // Alignment (Alignment uses size so we should always align AFTER resizing)
                RefreshDCLAlignmentAndPosition(parentRT);
                m_UIShapeRefreshDCLAlignmentAndPosition.End();
            }

            m_UIShapeRefreshDCLLayout.End();
        }

        protected virtual void RefreshDCLSize(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();

            Model model = (Model) this.model;

            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                model.width.GetScaledValue(parentTransform.rect.width));
            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                model.height.GetScaledValue(parentTransform.rect.height));
        }

        private void RefreshDCLAlignmentAndPosition(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();

            referencesContainer.layoutElement.ignoreLayout = false;
            ConfigureAlignment(referencesContainer.layoutGroup);
            Utils.ForceRebuildLayoutImmediate(parentTransform);
            referencesContainer.layoutElement.ignoreLayout = true;

            Model model = (Model) this.model;

            // Reposition
            Vector3 position = Vector3.zero;
            position.x = model.positionX.GetScaledValue(parentTransform.rect.width);
            position.y = model.positionY.GetScaledValue(parentTransform.rect.height);

            position = Utils.Sanitize(position);
            referencesContainer.layoutElementRT.localPosition += position;
        }

        protected virtual void RefreshDCLLayoutRecursively(bool refreshSize = true, bool refreshAlignmentAndPosition = true) =>
            RefreshDCLLayoutRecursively_Internal(refreshSize, refreshAlignmentAndPosition);

        private void RefreshDCLLayoutRecursively_Internal(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            if (rootParent.referencesContainer == null)
                return;

            Utils.InverseTransformChildTraversal<UIReferencesContainer>(
                (x) =>
                {
                    if (x.owner != null)
                        x.owner.RefreshDCLLayout(refreshSize, refreshAlignmentAndPosition);
                },
                rootParent.referencesContainer.transform);
        }

        private void FixMaxStretchRecursively()
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            if (rootParent.referencesContainer == null)
                return;

            Utils.InverseTransformChildTraversal<UIReferencesContainer>(
                (x) =>
                {
                    if (x.owner != null)
                    {
                        x.rectTransform.SetToMaxStretch();
                    }
                },
                rootParent.referencesContainer.transform);
        }

        protected bool ReparentComponent(RectTransform targetTransform, string targetParent)
        {
            bool targetParentExists = !string.IsNullOrEmpty(targetParent) &&
                                      scene.componentsManagerLegacy.HasSceneSharedComponent(targetParent);

            if (targetParentExists && parentUIComponent == scene.componentsManagerLegacy.GetSceneSharedComponent(targetParent))
            {
                return false;
            }

            if (parentUIComponent != null)
            {
                UIReferencesContainer[] parents = referencesContainer.GetComponentsInParent<UIReferencesContainer>(true);

                foreach (var parent in parents)
                {
                    if (parent.owner != null)
                    {
                        parent.owner.OnChildDetached(parentUIComponent, this);
                    }
                }
            }

            if (targetParentExists)
            {
                parentUIComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(targetParent) as UIShape;
            }
            else
            {
                parentUIComponent = scene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();
            }

            targetTransform.SetParent(parentUIComponent.childHookRectTransform, false);
            return true;
        }

        public UIShape GetRootParent()
        {
            UIShape parent = null;

            if (parentUIComponent != null && !(parentUIComponent is UIScreenSpace))
            {
                parent = parentUIComponent.GetRootParent();
            }
            else
            {
                parent = this;
            }

            return parent;
        }

        private void ConfigureAlignment(LayoutGroup layout)
        {
            Model model = (Model) this.model;

            layout.childAlignment = model.vAlign switch
                                    {
                                        "top" => model.hAlign switch
                                                 {
                                                     "left" => TextAnchor.UpperLeft,
                                                     "right" => TextAnchor.UpperRight,
                                                     _ => TextAnchor.UpperCenter
                                                 },
                                        "bottom" => model.hAlign switch
                                                    {
                                                        "left" => TextAnchor.LowerLeft,
                                                        "right" => TextAnchor.LowerRight,
                                                        _ => TextAnchor.LowerCenter
                                                    },
                                        _ => model.hAlign switch
                                             {
                                                 "left" => TextAnchor.MiddleLeft,
                                                 "right" => TextAnchor.MiddleRight,
                                                 _ => TextAnchor.MiddleCenter
                                             }
                                    };
        }

        protected void SetComponentDebugName()
        {
            if (referencesContainer != null && model != null)
                referencesContainer.name = componentName;
        }

        public override void Dispose()
        {
            if (childHookRectTransform)
                Utils.SafeDestroy(childHookRectTransform.gameObject);

            if (referencesContainer != null)
                pool.ReleaseUIShape(referencesContainer);

            screenSize.OnChange -= OnScreenResize;

            base.Dispose();
        }

        public virtual void OnChildAttached(UIShape _, UIShape childComponent) { }

        protected virtual void OnChildDetached(UIShape parentComponent, UIShape childComponent) { }

        public void Refresh()
        {
            RefreshRecursively();
            isLayoutDirty = false;

            OnLayoutRefresh?.Invoke();
            OnLayoutRefresh = null;
        }
    }
}
