using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;

namespace DCL.Components
{
    public class SmartItemComponent : BaseComponent
    {
        public class Model
        {
            public Dictionary<object, object> values;
        }

        public Model model;

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            Model newModel = Utils.FromJsonWithNulls<Model>(newJson);
            model = newModel;
            yield break;
        }

        public override void SetModel(object model)
        {
            this.model = (Model)model;
        }

    }
}