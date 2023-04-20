using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicListComponentView : BaseComponentView, IDynamicListComponentView
{
    [SerializeField] private Image iconReference;

    internal List<Image> instantiatedIcons = new ();
    public override void RefreshControl()
    {
    }

    public void AddIcon(Sprite sprite)
    {
        Image newImage = Instantiate(iconReference, transform);
        newImage.sprite = sprite;
        instantiatedIcons.Add(newImage);
    }

    public void AddIcons(List<Sprite> spriteList)
    {
        RemoveIcons();

        foreach (var sprite in spriteList)
        {
            AddIcon(sprite);
        }
    }

    public void RemoveIcons()
    {
        foreach (var icon in instantiatedIcons)
            Destroy(icon.gameObject);

        instantiatedIcons = new List<Image>();
    }
}
