using System.Collections;
using DCL.Helpers;

namespace DCL.Components
{
    public class DCLGizmos : BaseComponent
    {
        public static class Gizmo
        {
            public const string MOVE = "MOVE";
            public const string ROTATE = "ROTATE";
            public const string SCALE = "SCALE";
            public const string NONE = "NONE";
        }

        [System.Serializable]
        public class Model
        {
            public bool position = true;
            public bool rotation = true;
            public bool scale = true;
            public bool cycle = true;
            public string selectedGizmo = Gizmo.NONE;
            public bool localReference = false;
        }

        public Model model;

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);
            yield return null;
        }
    }
}