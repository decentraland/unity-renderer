using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewAllComponentView : BaseComponentView, IViewAllComponentView
{
    private const int ELEMENTS_PER_PAGE = 20;

    [SerializeField] private TMP_Text sectionName;
    [SerializeField] private ButtonComponentView backButton;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private UIPageSelector pageSelector;

    public event Action OnBackFromViewAll;
    public event Action<int, int> OnRequestCollectibleElements;

    public override void Start()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackFromViewAll?.Invoke());
        pageSelector.OnValueChanged += RequestPage;
    }

    public void Initialize(int totalCollectiblesElements)
    {
        pageSelector.Setup(totalCollectiblesElements/ELEMENTS_PER_PAGE, false);
    }

    private void RequestPage(int pageNumber)
    {
        OnRequestCollectibleElements?.Invoke(pageNumber, ELEMENTS_PER_PAGE);
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
