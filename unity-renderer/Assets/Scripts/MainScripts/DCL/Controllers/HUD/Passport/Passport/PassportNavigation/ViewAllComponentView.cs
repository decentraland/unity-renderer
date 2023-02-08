using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewAllComponentView : BaseComponentView, IViewAllComponentView
{
    [SerializeField] private TMP_Text sectionName;
    [SerializeField] private ButtonComponentView backButton;
    [SerializeField] private Transform itemsContainer;

    public event Action OnBackFromViewAll;

    public override void Start()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackFromViewAll?.Invoke());
    }

    public override void RefreshControl()
    {
    }

    public void SetSectionName(string sectionNameText)
    {
        sectionName.text = sectionNameText;
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
