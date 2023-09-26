using UnityEngine;

namespace DCL.ContentModeration
{
    public class AdultContentEnabledNotificationComponentView : BaseComponentView, IAdultContentEnabledNotificationComponentView
    {
        [SerializeField] internal ButtonComponentView closeButton;

        public override void Awake()
        {
            base.Awake();
            closeButton.onClick.AddListener(() => Hide());
        }

        public override void Dispose()
        {
            closeButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowNotification() =>
            Show();

        public void HideNotification() =>
            Hide();
    }
}
