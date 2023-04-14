using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIPageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly int IS_ACTIVE_ANIMATOR_HASH = Animator.StringToHash("IsActive");
        private static readonly int IS_HOVER_ANIMATOR_HASH = Animator.StringToHash("IsHover");

        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        [SerializeField] private Animator anim;

        private int pageNumber;

        public event Action<int> OnPageClicked;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonDown);
        }

        private void OnButtonDown()
        {
            OnPageClicked?.Invoke(pageNumber);
        }

        public void Initialize(int i)
        {
            pageNumber = i;
            text.text = (i + 1).ToString();
        }

        public void Toggle(bool b)
        {
            anim.SetBool(IS_ACTIVE_ANIMATOR_HASH, b);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            anim.SetBool(IS_HOVER_ANIMATOR_HASH, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            anim.SetBool(IS_HOVER_ANIMATOR_HASH, false);
        }
    }
}
