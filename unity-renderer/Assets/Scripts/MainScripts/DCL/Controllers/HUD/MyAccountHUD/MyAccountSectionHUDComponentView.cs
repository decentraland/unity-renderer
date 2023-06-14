using UnityEngine;

namespace DCL.MyAccount
{
    public class MyAccountSectionHUDComponentView : BaseComponentView, IMyAccountSectionHUDComponentView
    {
        [SerializeField] internal MyProfileComponentView myProfileComponentView;

        public IMyProfileComponentView CurrentMyProfileView => myProfileComponentView;

        private Transform thisTransform;

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
    }
}
