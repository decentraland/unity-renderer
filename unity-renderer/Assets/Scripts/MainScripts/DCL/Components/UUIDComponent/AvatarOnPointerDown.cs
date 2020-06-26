using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class AvatarOnPointerDown : OnPointerDown
    {
        new public Collider collider;
        public event System.Action OnPointerDownReport;

        void Awake()
        {
            CommonScriptableObjects.playerInfoCardVisibleState.OnChange += ReEnableOnInfoCardClosed;
        }

        void OnDestroy()
        {
            CommonScriptableObjects.playerInfoCardVisibleState.OnChange -= ReEnableOnInfoCardClosed;
        }

        public override void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled) return;

            if (ShouldReportEvent(buttonId, hit))
            {
                base.Report(buttonId, ray, hit);

                SetHoverState(false);
                enabled = false;

                OnPointerDownReport?.Invoke();
            }
        }

        void ReEnableOnInfoCardClosed(bool newState, bool prevState)
        {
            if (enabled || newState) return;

            enabled = true;
        }
    }
}