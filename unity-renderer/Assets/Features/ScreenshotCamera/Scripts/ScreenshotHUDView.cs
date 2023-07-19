using UnityEngine;
using UnityEngine.UI;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    [RequireComponent(typeof(Canvas)), DisallowMultipleComponent]
    public class ScreenshotHUDView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public Image RefImage { get; private set;}

        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        public void SwitchVisibility(bool isVisible)
        {
            canvas.enabled = isVisible;
        }
    }
}
