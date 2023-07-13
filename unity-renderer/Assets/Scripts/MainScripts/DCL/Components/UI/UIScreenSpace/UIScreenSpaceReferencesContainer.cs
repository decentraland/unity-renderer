using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScreenSpaceReferencesContainer : UIReferencesContainer
    {
        [Header("UIScreenSpace Fields")]
        public Canvas UiScreenSpaceCanvas;
        public CanvasScaler UiScreenSpaceCanvasScaler;
        public GraphicRaycaster UiScreenSpaceGraphicRaycaster;
    }
}
