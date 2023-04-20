using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.Scripts.Components
{
    public class PageSelectorButtonComponentView : BaseComponentView<PageSelectorButtonModel>
    {
        private static readonly int IS_ACTIVE_ANIMATOR_HASH = Animator.StringToHash("IsActive");
        private static readonly int IS_HOVER_ANIMATOR_HASH = Animator.StringToHash("IsHover");

        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        [SerializeField] private Animator anim;

        private int pageNumber;

        public event Action<int> OnPageClicked;

        public override void Awake()
        {
            base.Awake();

            onFocused += focused =>
            {
                anim.SetBool(IS_HOVER_ANIMATOR_HASH, focused);
            };

            button.onClick.AddListener(OnButtonDown);
        }

        public override void RefreshControl()
        {
            pageNumber = model.PageNumber;
            text.text = model.PageNumber.ToString();
        }

        private void OnButtonDown()
        {
            OnPageClicked?.Invoke(pageNumber);
        }

        public void Toggle(bool isOn)
        {
            anim.SetBool(IS_ACTIVE_ANIMATOR_HASH, isOn);
        }
    }
}
