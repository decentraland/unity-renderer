using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.Scripts.Components
{
    public class PageSelectorButtonComponentView : BaseComponentView
    {
        public event Action<int> OnPageClicked;

        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        [SerializeField] private Animator anim;

        private static readonly int IS_ACTIVE = Animator.StringToHash("IsActive");
        private static readonly int IS_HOVER = Animator.StringToHash("IsHover");

        private int pageNumber;

        public override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(OnButtonDown);
        }

        public override void RefreshControl() { }

        public void Initialize(int i)
        {
            pageNumber = i;
            text.text = (i + 1).ToString();
        }

        public void Toggle(bool b) =>
            anim.SetBool(IS_ACTIVE, b);

        private void OnButtonDown() =>
            OnPageClicked?.Invoke(pageNumber);

        public override void OnFocus() =>
            anim.SetBool(IS_HOVER, true);

        public override void OnLoseFocus() =>
            anim.SetBool(IS_HOVER, false);
    }
}
