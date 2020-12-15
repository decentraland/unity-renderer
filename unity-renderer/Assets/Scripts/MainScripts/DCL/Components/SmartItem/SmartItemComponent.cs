using System.Collections;
using DCL.Controllers;

namespace DCL.Components
{
    public class SmartItemComponent : BaseComponent
    {
        public override object GetModel()
        {
            return null;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield return null;
        }
    }
}