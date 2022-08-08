using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<int> OnPageClicked;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;
    [SerializeField] private Animator anim;
    private static readonly int isActive = Animator.StringToHash("IsActive");
    private static readonly int isHover = Animator.StringToHash("IsHover");
    private int pageNumber;

    private void Awake() { button.onClick.AddListener(OnButtonDown); }
    private void OnButtonDown() { OnPageClicked?.Invoke(pageNumber); }
    public void Initialize(int i)
    {
        pageNumber = i;
        text.text = (i + 1).ToString();
    }

    public void Toggle(bool b) { anim.SetBool(isActive, b); }
    public void OnPointerEnter(PointerEventData eventData) { anim.SetBool(isHover, true); }
    public void OnPointerExit(PointerEventData eventData) { anim.SetBool(isHover, false); }
}