using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceCategoryButton : MonoBehaviour
{
    [SerializeField] internal Button button;
    [SerializeField] internal Image buttonBackgroundImage;
    [SerializeField] internal Image iconImage;
    [SerializeField] internal TMP_Text text;
    [SerializeField] internal Color selectedBackgroundColor;
    [SerializeField] internal Color selectedTextColor;
    [SerializeField] internal Color deselectedBackgroundColor;
    [SerializeField] internal Color deselectedTextColor;

    [SerializeField] internal PlaceCategoryIcons cagtegoryIconsSO;

    public event Action<string, bool> OnClick;

    public string currenCategory { get; private set; }

    private bool isCurrentlySelected;

    private void Awake()
    {
        if (button == null)
            return;

        button.onClick.AddListener(() => OnClick?.Invoke(currenCategory, !isCurrentlySelected));
    }

    public void SetCategory(string categoryId, string nameToShow)
    {
        currenCategory = categoryId;
        text.text = RemoveNonASCIICharacters(nameToShow)
                             .ToUpper()
                             .Trim();

        var iconFound = false;
        foreach (PlaceCategoryIcon categoryIcon in cagtegoryIconsSO.categoryIcons)
        {
            if (categoryIcon.category != categoryId)
                continue;

            iconImage.sprite = categoryIcon.icon;
            iconFound = true;
            break;
        }

        iconImage.gameObject.SetActive(iconFound);
    }

    public void SetStatus(bool isSelected)
    {
        isCurrentlySelected = isSelected;
        buttonBackgroundImage.color = isSelected ? selectedBackgroundColor : deselectedBackgroundColor;
        text.color = isSelected ? selectedTextColor : deselectedTextColor;
    }

    private static string RemoveNonASCIICharacters(string text)
    {
        StringBuilder sb = new StringBuilder();

        foreach (char c in text)
            if (c <= 127) sb.Append(c);

        return sb.ToString();
    }
}
