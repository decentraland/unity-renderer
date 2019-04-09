using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIContainerStack : UIShape
    {
        public enum StackOrientation
        {
            VERTICAL,
            HORIZONTAL
        }

        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public Color color = Color.black;
            public float opacity = 1f;
            public StackOrientation stackOrientation = StackOrientation.VERTICAL;
            public bool adaptWidth = false;
            public bool adaptHeight = false;
        }

        new Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        new public UIContainerRectReferencesContainer referencesContainer
        {
            get { return base.referencesContainer as UIContainerRectReferencesContainer; }
            set { base.referencesContainer = value; }
        }

        public override string componentName => "UIContainerStack";
        HorizontalOrVerticalLayoutGroup layoutGroup;

        public UIContainerStack(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UIContainerStack attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<UIContainerRectReferencesContainer>("UIContainerRect");
                referencesContainer.name = "UIContainerStack";
            }
            else
            {
                ReparentComponent(referencesContainer.rectTransform, model.parentComponent);
            }

            referencesContainer.image.enabled = model.visible;

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            yield return ResizeAlignAndReposition(childHookRectTransform, parentRecTransform.rect.width, parentRecTransform.rect.height,
                                                referencesContainer.alignmentLayoutGroup,
                                                referencesContainer.imageLayoutElement);

            referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.opacity);

            referencesContainer.image.raycastTarget = model.isPointerBlocker;

            if (model.stackOrientation == StackOrientation.VERTICAL && !(layoutGroup is VerticalLayoutGroup))
            {
                Utils.SafeDestroy(layoutGroup);
                yield return null;

                layoutGroup = childHookRectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            else if (model.stackOrientation == StackOrientation.HORIZONTAL && !(layoutGroup is HorizontalLayoutGroup))
            {
                Utils.SafeDestroy(layoutGroup);
                yield return null;

                layoutGroup = childHookRectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter sizeFitter = childHookRectTransform.gameObject.GetComponent<ContentSizeFitter>();
            if (model.adaptWidth || model.adaptHeight)
            {
                if (sizeFitter == null)
                    sizeFitter = childHookRectTransform.gameObject.AddComponent<ContentSizeFitter>();

                sizeFitter.horizontalFit = model.adaptWidth ? ContentSizeFitter.FitMode.MinSize : ContentSizeFitter.FitMode.Unconstrained;
                sizeFitter.verticalFit = model.adaptHeight ? ContentSizeFitter.FitMode.MinSize : ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (sizeFitter != null)
            {
                Utils.SafeDestroy(sizeFitter);
                yield return null;
            }
        }

        protected override void OnChildComponentAttached(UIShape childComponent)
        {
            ContentSizeFitter sizeFitter = childComponent.referencesContainer.gameObject.GetOrCreateComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(referencesContainer.rectTransform);
        }

        protected override void OnChildComponentDettached(UIShape childComponent)
        {
            ContentSizeFitter sizeFitter = childComponent.referencesContainer.gameObject.GetComponent<ContentSizeFitter>();

            if (sizeFitter != null)
                GameObject.DestroyImmediate(sizeFitter);

            LayoutRebuilder.ForceRebuildLayoutImmediate(referencesContainer.rectTransform);
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}