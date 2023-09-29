using DG.Tweening;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    public class PlayerMarkerCircleAnimation : MonoBehaviour
    {
        [Space]
        [SerializeField] private Transform circle;

        [SerializeField] private float endScaleFactor = 0.85f;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private Ease easyType = Ease.InOutQuart;

        private Vector3 startScale;
        private Vector3 endScale;

        private Tween tween;

        private void Start()
        {
            startScale = circle.localScale;
            endScale = startScale * endScaleFactor;

            if (tween == null)
                StartPingPongAnimation();
        }

        private void OnEnable()
        {
            if (startScale == Vector3.zero) return;

            StartPingPongAnimation();
        }

        private void OnDisable()
        {
            if (tween == null || !tween.IsActive()) return;

            tween.Kill();
            circle.localScale = startScale;
        }

        [ContextMenu(nameof(StartPingPongAnimation))]
        private void StartPingPongAnimation()
        {
            tween = circle.DOScale(endScale, animationDuration)
                          .SetLoops(-1, LoopType.Yoyo) // -1 for infinite loops
                          .SetEase(easyType);
        }
    }
}
