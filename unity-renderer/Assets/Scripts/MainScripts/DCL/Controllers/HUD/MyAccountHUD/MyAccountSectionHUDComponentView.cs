using System.Collections;
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
        private Coroutine accountSettingsUpdatedToastRoutine;

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;
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
            if (accountSettingsUpdatedToastRoutine != null)
                StopCoroutine(accountSettingsUpdatedToastRoutine);

            accountSettingsUpdatedToastRoutine = StartCoroutine(ShowAccountSettingsUpdatedToastCoroutine());
        }

        private IEnumerator ShowAccountSettingsUpdatedToastCoroutine()
        {
            if (!accountSettingsUpdatedToast.gameObject.activeSelf)
                accountSettingsUpdatedToast.gameObject.SetActive(true);

            accountSettingsUpdatedToast.Show();
            yield return new WaitForSeconds(COPY_TOAST_VISIBLE_TIME);
            accountSettingsUpdatedToast.Hide();
        }
    }
}
