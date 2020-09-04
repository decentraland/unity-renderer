using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Categories = WearableLiterals.Categories;
using Rarity = WearableLiterals.ItemRarity;

public class AvatarEditorHUDAudioHandler : MonoBehaviour
{
    [SerializeField]
    AvatarEditorHUDView view;
    [SerializeField]
    Button randomizeButton;

    AudioContainer audioContainer;
    AudioEvent eventRarity;

    WearableItem lastClickedWearable = null;

    private void Start()
    {
        audioContainer = GetComponent<AudioContainer>();
        audioContainer.GetEvent("AvatarAppear").SetPitch(0.4f);

        eventRarity = audioContainer.GetEvent("Rarity");

        int nPairs = view.wearableGridPairs.Length;
        for (int i = 0; i < nPairs; i++)
        {
            view.wearableGridPairs[i].selector.OnItemClicked += OnWearableClicked;
        }

        view.skinColorSelector.OnColorChanged += OnSkinColorClicked;
        view.eyeColorSelector.OnColorChanged += OnEyeColorClicked;
        view.hairColorSelector.OnColorChanged += OnHairColorClicked;

        if (randomizeButton != null)
            randomizeButton.onClick.AddListener(ResetLastClickedWearable);
    }

    void OnWearableClicked(string wearableId)
    {
        var wearable = CatalogController.wearableCatalog.Get(wearableId);
        lastClickedWearable = wearable;
        if (wearable == null) return;

        switch (wearable.category)
        {
            case Categories.EYEBROWS:
                Play("Hair");
                break;
            case "facial_hair":
                Play("Hair");
                break;
            case Categories.FEET:
                Play("Footwear");
                break;
            case Categories.HAIR:
                Play("Hair");
                break;
            case Categories.LOWER_BODY:
                Play("Clothing");
                break;
            case Categories.UPPER_BODY:
                Play("Clothing");
                break;
            case "eyewear":
                Play("Eyewear");
                break;
            case "tiara":
                Play("Jewelry");
                break;
            case "earring":
                Play("Jewelry");
                break;
            case "hat":
                Play("Hat");
                break;
            case "top_head":
                Play("Footwear");
                break;
            case "helmet":
                Play("Footwear");
                break;
            case "mask":
                Play("Hat");
                break;
            default:
                break;
        }
    }

    void ResetLastClickedWearable()
    {
        lastClickedWearable = null;
    }

    void OnSkinColorClicked(Color color)
    {

    }

    void OnEyeColorClicked(Color color)
    {

    }

    void OnHairColorClicked(Color color)
    {

    }

    void Play(string eventName)
    {
        audioContainer.GetEvent(eventName).Play(true);
    }

    public void PlayRarity()
    {
        if (lastClickedWearable == null)
            return;

        if (lastClickedWearable.rarity == null)
            return;

        /*switch (lastClickedWearable.rarity)
        {
            case Rarity.RARE:
                eventRarity.SetIndex(0);
                break;
            case Rarity.EPIC:
                eventRarity.SetIndex(1);
                break;
            case Rarity.LEGENDARY:
                eventRarity.SetIndex(2);
                break;
            case Rarity.MYTHIC:
                eventRarity.SetIndex(3);
                break;
            case Rarity.UNIQUE:
                eventRarity.SetIndex(4);
                break;
            default:
                eventRarity.SetIndex(0);
                break;
        }*/

        eventRarity.Play(true);
    }
}
