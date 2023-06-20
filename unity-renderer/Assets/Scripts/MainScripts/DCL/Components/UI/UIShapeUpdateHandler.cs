using System.Collections;
using Unity.Profiling;

namespace DCL.Components
{
    public class UIShapeUpdateHandler<ReferencesContainerType, ModelType> : ComponentUpdateHandler
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model

    {
        public UIShape<ReferencesContainerType, ModelType> uiShapeOwner;

        public UIShapeUpdateHandler(IDelayedComponent owner) : base(owner) { uiShapeOwner = owner as UIShape<ReferencesContainerType, ModelType>; }

        ProfilerMarker m_PreApplyChanges = new ("VV.UIShapeUpdateHandler.PreApplyChanges");

        public override IEnumerator ApplyChangesWrapper(BaseModel newModel)
        {
            m_PreApplyChanges.Begin();
            uiShapeOwner.PreApplyChanges(newModel);
            m_PreApplyChanges.End();

            var enumerator = base.ApplyChangesWrapper(newModel);

            if (enumerator != null)
            {
                    yield return enumerator;
            }
        }
    }
}
