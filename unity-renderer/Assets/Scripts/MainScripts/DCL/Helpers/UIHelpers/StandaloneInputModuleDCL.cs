using UnityEngine.EventSystems;

namespace MainScripts.DCL.Helpers.UIHelpers
{
    public class StandaloneInputModuleDCL : StandaloneInputModule
    {
        public PointerEventData GetPointerData() =>
            m_PointerData[kMouseLeftId];
    }
}
