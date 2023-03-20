using UnityEngine.EventSystems;

namespace MainScripts.DCL.Helpers.UIHelpers
{
    public class StandaloneInputModuleDCL : StandaloneInputModule
    {
        public PointerEventData GetPointerData()
        {
            GetPointerData(kMouseLeftId, out PointerEventData data, true);
            return data;
        }
    }
}
