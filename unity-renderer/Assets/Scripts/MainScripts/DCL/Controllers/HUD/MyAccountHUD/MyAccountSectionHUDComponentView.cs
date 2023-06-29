using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.MyAccount
{
    public class MyAccountSectionHUDComponentView : BaseComponentView, IMyAccountSectionHUDComponentView
    {
        private const float COPY_TOAST_VISIBLE_TIME = 3;

        [SerializeField] internal MyProfileComponentView myProfileComponentView;
        [SerializeField] internal ShowHideAnimator accountSettingsUpdatedToast;

        public IMyProfileComponentView CurrentMyProfileView => myProfileComponentView;

        private Transform thisTransform;
        private CancellationTokenSource showAccountSettingsCancellationToken = new ();

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;
        }

        public override void Dispose()
        {
            base.Dispose();

            showAccountSettingsCancellationToken.SafeCancelAndDispose();
        }

        public override void RefreshControl() { }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            thisTransform.SetParent(parentTransform);
            thisTransform.localScale = Vector3.one;

            RectTransform rectTransform = thisTransform as RectTransform;
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public void ShowAccountSettingsUpdatedToast()
        {
            async UniTaskVoid ShowAccountSettingsUpdatedToastAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!accountSettingsUpdatedToast.gameObject.activeSelf)
                    accountSettingsUpdatedToast.gameObject.SetActive(true);

                accountSettingsUpdatedToast.Show();
                await UniTask.Delay(TimeSpan.FromSeconds(COPY_TOAST_VISIBLE_TIME), cancellationToken: cancellationToken);
                accountSettingsUpdatedToast.Hide();
            }

            showAccountSettingsCancellationToken = showAccountSettingsCancellationToken.SafeRestart();
            ShowAccountSettingsUpdatedToastAsync(showAccountSettingsCancellationToken.Token).Forget();
        }
    }
}
