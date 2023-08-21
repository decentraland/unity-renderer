using System.Linq;
using UnityEngine;

namespace DCL.Components.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class MaskCanvasRenderer : MonoBehaviour
    {
        [SerializeField] private RectTransform maskRectTransform;
        [SerializeField] private Vector2 offset;

        public RectTransform MaskRectTransform
        {
            get => maskRectTransform;

            set
            {
                maskRectTransform = value;

                if (enabled)
                    SetTargetClippingRect();
            }
        }

        private CanvasRenderer canvasRenderer;
        private Canvas rootCanvas;

        private void Awake()
        {
            canvasRenderer = GetComponent<CanvasRenderer>();
            rootCanvas = transform.GetComponentsInParent<Canvas>().FirstOrDefault(canvas => canvas.isRootCanvas);
        }

        private void OnEnable()
        {
            SetTargetClippingRect();
        }

        private void OnDisable()
        {
            canvasRenderer.DisableRectClipping();
        }

        private void OnRectTransformDimensionsChange()
        {
            SetTargetClippingRect();
        }

        private void SetTargetClippingRect()
        {
            if (rootCanvas == null) return;
            if (canvasRenderer == null) return;
            if (maskRectTransform == null) return;

            Rect rect = maskRectTransform.rect;
            // Get local position of maskRect as if it was direct child of root canvas, then offset mask rect by that amount
            rect.center += (Vector2)rootCanvas.transform.InverseTransformPoint(maskRectTransform.position);
            rect.center += offset;
            canvasRenderer.EnableRectClipping(rect);
        }
    }
}
