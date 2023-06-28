using System.Collections;

namespace DCL.Components
{
    public class UIShapeUpdateHandler<ReferencesContainerType, ModelType> : ComponentUpdateHandler
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model

    {
        private readonly UIShape<ReferencesContainerType, ModelType> uiShapeOwner;

        public UIShapeUpdateHandler(IDelayedComponent owner) : base(owner)
        {
            uiShapeOwner = owner as UIShape<ReferencesContainerType, ModelType>;
        }

        protected override IEnumerator ApplyChangesWrapper(BaseModel newModel)
        {
            uiShapeOwner.PreApplyChanges(newModel);

            var enumerator = base.ApplyChangesWrapper(newModel);

            if (enumerator != null)
                yield return enumerator;
        }
    }
}
