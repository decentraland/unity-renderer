using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
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
            float tmpValue = value;

            if (type == Unit.PERCENT)
            {
                tmpValue /= 100;
            }

            return tmpValue * (type == Unit.PIXELS ? 1 : parentSize);
        }
    }

    public class UIShape<ReferencesContainerType, ModelType> : UIShape
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model
    {
        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        new public ModelType model
        {
            get { return base.model as ModelType; }
            set { base.model = value; }
        }

        new public ReferencesContainerType referencesContainer
        {
            get { return base.referencesContainer as ReferencesContainerType; }
            set { base.referencesContainer = value; }
        }

        public override IEnumerator ApplyChangesWrapper(string newJson)
        {
            base.ApplyChangesWrapper(newJson);

            model = JsonUtility.FromJson<ModelType>(newJson);

            bool raiseOnAttached = false;

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<ReferencesContainerType>(referencesContainerPrefabName);
                raiseOnAttached = true;
            }
            else
            {
                if (ReparentComponent(referencesContainer.rectTransform, model.parentComponent))
                {
                    raiseOnAttached = true;
                }
            }

            var enumerator = ApplyChanges(newJson);

            if (enumerator != null)
            {
                yield return enumerator;
            }

            RefreshDCLLayout();
#if UNITY_EDITOR
            SetComponentDebugName();
#endif

            if (referencesContainer != null)
            {
                referencesContainer.canvasGroup.alpha = model.visible == false ? 0 : model.opacity;
                referencesContainer.canvasGroup.blocksRaycasts = model.isPointerBlocker;
            }

            RaiseOnAppliedChanges();

            if (raiseOnAttached && parentUIComponent != null)
            {
                UIReferencesContainer[] parents = referencesContainer.GetComponentsInParent<UIReferencesContainer>();

                foreach (var parent in parents)
                {
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
        public class Model
        {
            public string name;
            public string parentComponent;
            public bool visible = true;
            public float opacity = 1;
            public string hAlign = "center";
            public string vAlign = "center";
            public UIValue width = new UIValue(100, UIValue.Unit.PIXELS);
            public UIValue height = new UIValue(100, UIValue.Unit.PIXELS);
            public UIValue positionX;
            public UIValue positionY;
            public bool isPointerBlocker = true;
        }

        public override string componentName => GetDebugName();
        public virtual string referencesContainerPrefabName => "";
        public UIReferencesContainer referencesContainer;
        public RectTransform childHookRectTransform;

        public Model model = new Model();
        protected UIShape parentUIComponent;

        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        public string GetDebugName()
        {
            if (string.IsNullOrEmpty(model.name))
                return GetType().Name;
            else
                return GetType().Name + " - " + model.name;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            return null;
        }

        protected T InstantiateUIGameObject<T>(string prefabPath) where T : UIReferencesContainer
        {
            GameObject uiGameObject = null;
            bool targetParentExists = !string.IsNullOrEmpty(model.parentComponent) && scene.disposableComponents.ContainsKey(model.parentComponent);

            if (targetParentExists)
            {
                if (scene.disposableComponents.ContainsKey(model.parentComponent))
                {
                    parentUIComponent = (scene.disposableComponents[model.parentComponent] as UIShape);
                }
                else
                {
                    parentUIComponent = scene.uiScreenSpace as UIShape;
                }
            }
            else
            {
                parentUIComponent = scene.uiScreenSpace as UIShape;
            }

            uiGameObject = UnityEngine.Object.Instantiate(Resources.Load(prefabPath), parentUIComponent.childHookRectTransform) as GameObject;
            referencesContainer = uiGameObject.GetComponent<T>();

            referencesContainer.rectTransform.SetToMaxStretch();

            childHookRectTransform = referencesContainer.childHookRectTransform;

            referencesContainer.owner = this;

            return referencesContainer as T;
        }

        public virtual void RefreshAll()
        {
            RefreshDCLLayoutRecursively();
            FixMaxStretchRecursively();
            RefreshDCLLayoutRecursively_Internal(refreshSize: false, refreshAlignmentAndPosition: true);
        }

        public virtual void RefreshDCLLayout(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            RectTransform parentRT = referencesContainer.GetComponentInParent<RectTransform>();

            if (refreshSize)
                RefreshDCLSize(parentRT);

            if (refreshAlignmentAndPosition)
            {
                // Alignment (Alignment uses size so we should always align AFTER resizing)
                RefreshDCLAlignmentAndPosition(parentRT);
            }
        }

        protected void RefreshDCLSize(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
            {
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();
            }

            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, model.width.GetScaledValue(parentTransform.rect.width));
            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, model.height.GetScaledValue(parentTransform.rect.height));

            referencesContainer.layoutElementRT.ForceUpdateRectTransforms();
        }

        public void RefreshDCLAlignmentAndPosition(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
            {
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();
            }

            referencesContainer.layoutElement.ignoreLayout = false;
            ConfigureAlignment(referencesContainer.layoutGroup);
            LayoutRebuilder.ForceRebuildLayoutImmediate(referencesContainer.layoutGroup.transform as RectTransform);
            referencesContainer.layoutElement.ignoreLayout = true;

            // Reposition
            Vector3 position = Vector3.zero;
            position.x = model.positionX.GetScaledValue(parentTransform.rect.width);
            position.y = model.positionY.GetScaledValue(parentTransform.rect.height);

            referencesContainer.layoutElementRT.localPosition += position;
        }

        public virtual void RefreshDCLLayoutRecursively(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            RefreshDCLLayoutRecursively_Internal(refreshSize, refreshAlignmentAndPosition);
        }

        public void RefreshDCLLayoutRecursively_Internal(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            Utils.InverseTreeTraversal<UIReferencesContainer>(
                (x) =>
                {
                    if (x.owner != null)
                    {
                        x.owner.RefreshDCLLayout(refreshSize, refreshAlignmentAndPosition);
                    }
                },
                rootParent.referencesContainer.transform);
        }

        public void FixMaxStretchRecursively()
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            Utils.InverseTreeTraversal<UIReferencesContainer>(
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
            bool targetParentExists = !string.IsNullOrEmpty(targetParent) && scene.disposableComponents.ContainsKey(targetParent);

            if (targetParentExists && parentUIComponent == scene.disposableComponents[targetParent])
            {
                return false;
            }

            if (parentUIComponent != null)
            {
                UIReferencesContainer[] parents = referencesContainer.GetComponentsInParent<UIReferencesContainer>();

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
                parentUIComponent = scene.uiScreenSpace as UIShape;
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
            Utils.SafeDestroy(childHookRectTransform.gameObject);
            base.Dispose();
        }

        public virtual void OnChildAttached(UIShape parentComponent, UIShape childComponent) { }
        public virtual void OnChildDetached(UIShape parentComponent, UIShape childComponent) { }
    }
}
