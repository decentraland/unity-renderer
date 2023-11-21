using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceCategoryButton : MonoBehaviour
{
    [SerializeField] internal Button button;
    [SerializeField] internal GameObject deselected;
    [SerializeField] internal Image deselectedImage;
    [SerializeField] internal TMP_Text deselectedText;
    [SerializeField] internal GameObject selected;
    [SerializeField] internal Image selectedImage;
    [SerializeField] internal TMP_Text selectedText;

    public event Action<string, bool> OnClick;

    public string currenCategory { get; private set; }

    private bool isCurrentlySelected;

    private void Awake() =>
        button.onClick.AddListener(() => OnClick?.Invoke(currenCategory, !isCurrentlySelected));

    public void SetCategory(string category) =>
        currenCategory = category;

    public void SetText(string text)
    {
        deselectedText.text = text;
        selectedText.text = text;
    }

    public void SetStatus(bool isSelected)
    {
        isCurrentlySelected = isSelected;
        button.targetGraphic = isSelected ? selectedImage : deselectedImage;
        deselected.SetActive(!isSelected);
        selected.SetActive(isSelected);
    }
}
