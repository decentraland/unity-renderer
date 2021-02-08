using System.Collections;
using DCL.Controllers;
using DCL.Helpers;

namespace DCL.Components
{
    public class SmartItemComponent : BaseComponent
    {
        public class Model
        {
            public SmartItemAction[] actions;
            public SmartItemParameter[] parameters;
        }

        public Model model;

        public override object GetModel()
        {
            return model;
        }

        public bool HasActions()
        {
            if (model.actions.Length > 0)
                return true;

            return false;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            UpdateModel(newJson);
            yield return null;
        }

        public void ForceUpdate(string json)
        {
            UpdateModel(json);
        }

        void UpdateModel(string json)
        {
            Model newModel = Utils.SafeFromJson<Model>(json);
            model = newModel;
        }
    }
}