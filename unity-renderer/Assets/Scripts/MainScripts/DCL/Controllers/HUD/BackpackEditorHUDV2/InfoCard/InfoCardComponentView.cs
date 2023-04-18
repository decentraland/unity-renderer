using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class InfoCardComponentView : BaseComponentView<InfoCardComponentModel>, IInfoCardComponentView
{
    [SerializeField] private TMP_Text wearableName;
    [SerializeField] private TMP_Text wearableDescription;
    [SerializeField] private Image categoryImage;
    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetName(model.name);
        SetDescription(model.description);
        SetCategory(model.category);
    }

    public void SetName(string name)
    {
        model.name = name;
        wearableName.text = name;
    }

    public void SetDescription(string description)
    {
        model.description = description;
        wearableDescription.text = description;
    }

    public void SetCategory(string category)
    {
        model.category = category;
        categoryImage.sprite = null;
    }
}
