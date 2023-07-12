
namespace DCL.Components
{
     public class UIShape<ReferencesContainerType, ModelType> : UIShape
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model
    {
        protected const float RAYCAST_ALPHA_THRESHOLD = 0.01f;

        protected UIShape(UIShapePool pool) : base(pool)
        {
        }

        public new ModelType model
        {
            get => base.model as ModelType;
            protected set => base.model = value;
        }

        public new ReferencesContainerType referencesContainer
        {
            get => base.referencesContainer as ReferencesContainerType;
            protected set => base.referencesContainer = value;
        }

        public override ComponentUpdateHandler CreateUpdateHandler() =>
            new UIShapeUpdateHandler<ReferencesContainerType, ModelType>(this);

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
                referencesContainer = GetUIGameObjectFromPool<ReferencesContainerType>();

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

                for (var i = 0; i < parents.Length; i++)
                    parents[i].owner?.OnChildAttached(parentUIComponent, this);
            }
        }
    }
}
