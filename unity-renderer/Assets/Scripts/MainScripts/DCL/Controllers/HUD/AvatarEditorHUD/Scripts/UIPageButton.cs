using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPageButton : MonoBehaviour
{
    public event Action<int> OnPageClicked;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;
    [SerializeField] private Animator anim;
    private static readonly int isActive = Animator.StringToHash("IsActive");
    private int pageNumber;

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
        text.text = (i+1).ToString();
    }
    public void Toggle(bool b)
    {
        anim.SetBool(isActive, b);
    }
}