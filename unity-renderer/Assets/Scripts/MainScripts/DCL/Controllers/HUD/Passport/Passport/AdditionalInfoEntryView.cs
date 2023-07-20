using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdditionalInfoEntryView : BaseComponentView
{
    [SerializeField] private Image logoImg;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text valueText;

    public void SetInfo(Sprite logo, string title, string value)
    {
        logoImg.sprite = logo;
        titleText.text = title;
        valueText.text = value;
    }

    public override void RefreshControl() { }
}
