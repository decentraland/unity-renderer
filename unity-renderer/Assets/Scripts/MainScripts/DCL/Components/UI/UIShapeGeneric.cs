using UnityEngine;

namespace DCL.Components
{
     public class UIShape<ReferencesContainerType, ModelType> : UIShape
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model
    {
        protected const float RAYCAST_ALPHA_THRESHOLD = 0.01f;

        protected UIShape(UIShapePool pool, UIShapeScheduler scheduler) : base(pool, scheduler)
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

        private bool raiseOnAttached;
        bool firstApplyChangesCall;

        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;

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

            bool isInsideSceneBounds = this.scene.IsInsideSceneBoundaries(dataStorePlayer.playerGridPosition.Get());

            // We hide the component visibility when it's not inside the scene bounds and it's not a child of another UI component

            // FD:: This is a hacky way to hide the component visibility when it's not inside the scene bounds and it's not a child of another UI component
            // but this triggers a wider refresh of all UIShape components and makes the spike in the Profiler.
            // if (!isInsideSceneBounds && referencesContainer.transform.parent == null)
            //     referencesContainer.SetVisibility(visible: false);
            // else if (isInsideSceneBounds && referencesContainer.transform.parent == null)
            //     referencesContainer.SetVisibility(model.visible, model.opacity);


            // FD:: This should be the "Normal" flow, but it's not working properly, so we were using the hacky above (commented) code.
            if (firstApplyChangesCall)
                referencesContainer.SetVisibility(visible: false);
            else
                referencesContainer.SetVisibility(model.visible, model.opacity);


            referencesContainer.SetBlockRaycast(model.visible && model.isPointerBlocker);

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
