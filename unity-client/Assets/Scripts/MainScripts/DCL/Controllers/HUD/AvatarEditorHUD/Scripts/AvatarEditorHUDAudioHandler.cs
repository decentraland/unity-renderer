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
    ItemSelector nftItemSelector;
    [SerializeField]
    Button randomizeButton;
    [SerializeField]
    AudioEvent eventMusic, eventRarity, eventAvatarAppear, eventReactionMale, eventReactionFemale, eventWearableClothing, eventWearableEyewear, eventWearableJewelry,
        eventWearableFootwear, eventWearableHair, eventWearableHatMask, eventWearableRarity;

    WearableItem lastSelectedWearable = null;
    bool hasClickedRandomize = false, wearableIsSameAsPrevious = false;

    IEnumerator musicFadeOut;

    private void Start()
    {
        int nPairs = view.wearableGridPairs.Length;
        for (int i = 0; i < nPairs; i++)
        {
            view.wearableGridPairs[i].selector.OnItemClicked += OnSelectWearable;
        }

        nftItemSelector.OnItemClicked += OnSelectWearable;

        view.OnSetVisibility += OnSetAvatarEditorVisibility;
        view.OnRandomize += OnClickRandomize;
    }

    void OnSelectWearable(string wearableId)
    {
        CatalogController.wearableCatalog.TryGetValue(wearableId, out var wearable);
        wearableIsSameAsPrevious = (wearable == lastSelectedWearable);
        if (wearableIsSameAsPrevious) return;

        lastSelectedWearable = wearable;
        if (wearable == null) return;

        switch (wearable.category)
        {
            case Categories.EYEBROWS:
                eventWearableHair.Play(true);
                break;
            case "facial_hair":
                eventWearableHair.Play(true);
                break;
            case Categories.FEET:
                eventWearableFootwear.Play(true);
                break;
            case Categories.HAIR:
                eventWearableHair.Play(true);
                break;
            case Categories.LOWER_BODY:
                eventWearableClothing.Play(true);
                break;
            case Categories.UPPER_BODY:
                eventWearableClothing.Play(true);
                break;
            case "eyewear":
                eventWearableEyewear.Play(true);
                break;
            case "tiara":
                eventWearableJewelry.Play(true);
                break;
            case "earring":
                eventWearableJewelry.Play(true);
                break;
            case "hat":
                eventWearableHatMask.Play(true);
                break;
            case "top_head":
                eventWearableFootwear.Play(true);
                break;
            case "helmet":
                eventWearableFootwear.Play(true);
                break;
            case "mask":
                eventWearableHatMask.Play(true);
                break;
            default:
                break;
        }
    }

    void OnEyeColorChanged(Color color)
    {
        ResetLastClickedWearable();
    }

    void OnSkinColorChanged(Color color)
    {
        ResetLastClickedWearable();
    }

    void OnHairColorChanged(Color color)
    {
        ResetLastClickedWearable();
    }

    void OnClickRandomize()
    {
        hasClickedRandomize = true;
        ResetLastClickedWearable();
    }

    void ResetLastClickedWearable()
    {
        lastSelectedWearable = null;
    }

    void OnAvatarAppear(AvatarModel model)
    {
        if (!view.isOpen) return;

        eventAvatarAppear.Play(true);

        if (!wearableIsSameAsPrevious)
        {
            if (lastSelectedWearable != null)
                PlayRarity();

            if (lastSelectedWearable != null || hasClickedRandomize)
                PlayVoiceReaction(model.bodyShape);
        }

        hasClickedRandomize = false;
    }

    void PlayRarity()
    {
        if (lastSelectedWearable.rarity == null)
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

        if (lastSelectedWearable.rarity == Rarity.UNIQUE)
            eventRarity.SetIndex(1);
        else
            eventRarity.SetIndex(0);

        eventRarity.Play(true);
    }

    void PlayVoiceReaction(string bodyShape)
    {
        float chanceToPlay = 0.75f;
        AudioEvent eventReaction = null;

        if (bodyShape.Contains("Female"))
            eventReaction = eventReactionFemale;
        else
            eventReaction = eventReactionMale;

        if (lastSelectedWearable != null)
        {
            if (lastSelectedWearable.rarity == Rarity.EPIC
                || lastSelectedWearable.rarity == Rarity.LEGENDARY
                || lastSelectedWearable.rarity == Rarity.MYTHIC
                || lastSelectedWearable.rarity == Rarity.UNIQUE)
            {
                eventReaction.RandomizeIndex(14, 28);
                chanceToPlay = 1f;
            }
            else
            {
                eventReaction.RandomizeIndex(0, 14);
            }
        }

        if (eventReaction != null && Random.Range(0f, 1f) <= chanceToPlay)
        {
            if (!eventReaction.source.isPlaying)
            {
                eventReaction.PlayScheduled(0.6f);
            }
        }
    }

    void OnSetAvatarEditorVisibility(bool visible)
    {
        AudioScriptableObjects.listItemAppear.ResetPitch();

        if (visible)
        {
            if (musicFadeOut != null)
            {
                StopCoroutine(musicFadeOut);
                StartCoroutine(eventMusic.FadeIn(1f));
            }

            if (!eventMusic.source.isPlaying)
                eventMusic.Play();

            view.eyeColorSelector.OnColorChanged += OnEyeColorChanged;
            view.skinColorSelector.OnColorChanged += OnSkinColorChanged;
            view.hairColorSelector.OnColorChanged += OnHairColorChanged;
            view.OnAvatarAppear += OnAvatarAppear;
        }
        else
        {
            musicFadeOut = eventMusic.FadeOut(2f);
            StartCoroutine(musicFadeOut);

            view.eyeColorSelector.OnColorChanged -= OnEyeColorChanged;
            view.skinColorSelector.OnColorChanged -= OnSkinColorChanged;
            view.hairColorSelector.OnColorChanged -= OnHairColorChanged;
            view.OnAvatarAppear -= OnAvatarAppear;
        }
    }
}
