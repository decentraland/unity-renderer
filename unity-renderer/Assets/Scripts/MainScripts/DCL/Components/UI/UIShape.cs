using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace DCL.Components
{
    [System.Serializable]
    public struct UIValue
    {
        public enum Unit
        {
            PERCENT,
            PIXELS
        }

        public float value;
        public Unit type;

        public void SetPixels(float value)
        {
            this.type = Unit.PIXELS;
            this.value = value;
        }

        public void SetPercent(float value)
        {
            this.type = Unit.PERCENT;
            this.value = value;
        }

        public UIValue(float value, Unit unitType = Unit.PIXELS)
        {
            this.value = value;
            this.type = unitType;
        }

        public float GetScaledValue(float parentSize)
        {
            if (type == Unit.PIXELS)
                return value;

            // Prevent division by zero
            if (parentSize <= Mathf.Epsilon)
                parentSize = 1;

            return value / 100 * parentSize;
        }
    }

    public class UIShape<ReferencesContainerType, ModelType> : UIShape
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model
    {
        public const float RAYCAST_ALPHA_THRESHOLD = 0.01f;

        public UIShape() { }

        new public ModelType model { get { return base.model as ModelType; } set { base.model = value; } }

        new public ReferencesContainerType referencesContainer { get { return base.referencesContainer as ReferencesContainerType; } set { base.referencesContainer = value; } }

        public override ComponentUpdateHandler CreateUpdateHandler() { return new UIShapeUpdateHandler<ReferencesContainerType, ModelType>(this); }

        bool raiseOnAttached;
        bool firstApplyChangesCall;

        /// <summary>
        /// This is called by UIShapeUpdateHandler before calling ApplyChanges.
        /// </summary>
        public void PreApplyChanges(BaseModel newModel)
        {
            model = (ModelType) newModel;

            raiseOnAttached = false;
            firstApplyChangesCall = false;

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<ReferencesContainerType>(referencesContainerPrefabName);

                raiseOnAttached = true;
                firstApplyChangesCall = true;
            }
            else if (ReparentComponent(referencesContainer.rectTransform, model.parentComponent))
            {
                raiseOnAttached = true;
            }
        }

        public override void RaiseOnAppliedChanges()
        {
            RefreshDCLLayout();

#if UNITY_EDITOR
            SetComponentDebugName();
#endif

            // We hide the component visibility when it's created (first applychanges)
            // as it has default values and appears in the middle of the screen
            if (firstApplyChangesCall)
                referencesContainer.canvasGroup.alpha = 0f;
            else
                referencesContainer.canvasGroup.alpha = model.visible ? model.opacity : 0f;

            referencesContainer.canvasGroup.blocksRaycasts = model.visible && model.isPointerBlocker;

            base.RaiseOnAppliedChanges();

            if (raiseOnAttached && parentUIComponent != null)
            {
                UIReferencesContainer[] parents = referencesContainer.GetComponentsInParent<UIReferencesContainer>(true);

                for (int i = 0; i < parents.Length; i++)
                {
                    UIReferencesContainer parent = parents[i];
                    if (parent.owner != null)
                    {
                        parent.owner.OnChildAttached(parentUIComponent, this);
                    }
                }
            }
        }
    }

    public class UIShape : BaseDisposable
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
            public UIValue width = new UIValue(100f);
            public UIValue height = new UIValue(50f);
            public UIValue positionX = new UIValue(0f);
            public UIValue positionY = new UIValue(0f);
            public bool isPointerBlocker = true;
            public string onClick;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        public override string componentName => GetDebugName();
        public virtual string referencesContainerPrefabName => "";
        public UIReferencesContainer referencesContainer;
        public RectTransform childHookRectTransform;

        public bool isLayoutDirty { get; protected set; }
        protected System.Action OnLayoutRefresh;

        private BaseVariable<Vector2Int> screenSize => DataStore.i.screen.size;

        public UIShape parentUIComponent { get; protected set; }

        private Coroutine layoutRefreshWatcher;

        public UIShape()
        {
            screenSize.OnChange += OnScreenResize;
            model = new Model();

            if ( layoutRefreshWatcher == null )
                layoutRefreshWatcher = CoroutineStarter.Start(LayoutRefreshWatcher());
        }

        private void OnScreenResize(Vector2Int current, Vector2Int previous)
        {
            isLayoutDirty = GetRootParent() == this;
        }

        public override int GetClassId() { return (int) CLASS_ID.UI_IMAGE_SHAPE; }

        public string GetDebugName()
        {
            Model model = (Model) this.model;

            if (string.IsNullOrEmpty(model.name))
            {
                return GetType().Name;
            }
            else
            {
                return GetType().Name + " - " + model.name;
            }
        }

        public override IEnumerator ApplyChanges(BaseModel newJson) { return null; }

        internal T InstantiateUIGameObject<T>(string prefabPath) where T : UIReferencesContainer
        {
            Model model = (Model) this.model;

            GameObject uiGameObject = null;

            bool targetParentExists = !string.IsNullOrEmpty(model.parentComponent) &&
                                      scene.disposableComponents.ContainsKey(model.parentComponent);

            if (targetParentExists)
            {
                if (scene.disposableComponents.ContainsKey(model.parentComponent))
                {
                    parentUIComponent = (scene.disposableComponents[model.parentComponent] as UIShape);
                }
                else
                {
                    parentUIComponent = scene.GetSharedComponent<UIScreenSpace>();
                }
            }
            else
            {
                parentUIComponent = scene.GetSharedComponent<UIScreenSpace>();
            }

            uiGameObject =
                Object.Instantiate(
                    Resources.Load(prefabPath),
                    parentUIComponent != null ? parentUIComponent.childHookRectTransform : null) as GameObject;

            referencesContainer = uiGameObject.GetComponent<T>();

            referencesContainer.rectTransform.SetToMaxStretch();

            childHookRectTransform = referencesContainer.childHookRectTransform;

            referencesContainer.owner = this;

            return referencesContainer as T;
        }

        IEnumerator LayoutRefreshWatcher()
        {
            while (true)
            {
                // WaitForEndOfFrame doesn't work in batch mode
                yield return Application.isBatchMode ? null : new WaitForEndOfFrame();

                if ( !isLayoutDirty )
                    continue;

                // When running tests this is empty.
                if (!string.IsNullOrEmpty(CommonScriptableObjects.sceneID))
                {
                    if (CommonScriptableObjects.sceneID.Get() != scene.sceneData.id)
                        continue;
                }

                RefreshAll();
            }
        }

        public virtual void RefreshAll()
        {
            // We are not using the _Internal here because the method is overridden
            // by some UI shapes.
            RefreshDCLLayoutRecursively(refreshSize: true, refreshAlignmentAndPosition: false);
            FixMaxStretchRecursively();
            RefreshDCLLayoutRecursively_Internal(refreshSize: false, refreshAlignmentAndPosition: true);
            isLayoutDirty = false;
            OnLayoutRefresh?.Invoke();
            OnLayoutRefresh = null;
        }

        public virtual void MarkLayoutDirty( System.Action OnRefresh = null )
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            if (rootParent.referencesContainer == null)
                return;

            rootParent.isLayoutDirty = true;

            if ( OnRefresh != null )
                rootParent.OnLayoutRefresh += OnRefresh;
        }

        public void RefreshDCLLayout(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            RectTransform parentRT = referencesContainer.GetComponentInParent<RectTransform>();

            if (refreshSize)
            {
                RefreshDCLSize(parentRT);
            }

            if (refreshAlignmentAndPosition)
            {
                // Alignment (Alignment uses size so we should always align AFTER resizing)
                RefreshDCLAlignmentAndPosition(parentRT);
            }
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

        public void RefreshDCLAlignmentAndPosition(RectTransform parentTransform = null)
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

        public virtual void RefreshDCLLayoutRecursively(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            RefreshDCLLayoutRecursively_Internal(refreshSize, refreshAlignmentAndPosition);
        }

        public void RefreshDCLLayoutRecursively_Internal(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
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

        public void FixMaxStretchRecursively()
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
                                      scene.disposableComponents.ContainsKey(targetParent);

            if (targetParentExists && parentUIComponent == scene.disposableComponents[targetParent])
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
                parentUIComponent = scene.disposableComponents[targetParent] as UIShape;
            }
            else
            {
                parentUIComponent = scene.GetSharedComponent<UIScreenSpace>();
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

        protected void ConfigureAlignment(LayoutGroup layout)
        {
            Model model = (Model) this.model;
            switch (model.vAlign)
            {
                case "top":
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.UpperLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.UpperRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.UpperCenter;
                            break;
                    }

                    break;
                case "bottom":
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.LowerLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.LowerRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.LowerCenter;
                            break;
                    }

                    break;
                default: // center
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.MiddleLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.MiddleRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.MiddleCenter;
                            break;
                    }

                    break;
            }
        }

        protected void SetComponentDebugName()
        {
            if (referencesContainer == null || model == null)
            {
                return;
            }

            referencesContainer.name = componentName;
        }

        public override void Dispose()
        {
            if (childHookRectTransform)
                Utils.SafeDestroy(childHookRectTransform.gameObject);

            screenSize.OnChange -= OnScreenResize;

            if ( layoutRefreshWatcher != null )
                CoroutineStarter.Stop(layoutRefreshWatcher);

            base.Dispose();
        }

        public virtual void OnChildAttached(UIShape parentComponent, UIShape childComponent) { }

        public virtual void OnChildDetached(UIShape parentComponent, UIShape childComponent) { }
    }
}