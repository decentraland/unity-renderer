using System.Collections;
using DCL.Controllers;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public interface IComponent
    {
        IParcelScene scene { get; }
        string componentName { get; }
        void UpdateFromJSON(string json);
        void UpdateFromModel(BaseModel model);
        void UpdateFromPb(ComponentBodyPayload model);
        IEnumerator ApplyChanges(BaseModel model);
        void RaiseOnAppliedChanges();
        bool IsValid();
        BaseModel GetModel();
        int GetClassId();
    }
}
