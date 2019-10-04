using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

public class AvatarEditorHUDController : IDisposable, IHUD
{
    private AvatarEditorHUDView view;
    private AvatarEditorHUDModel model { get; } = new AvatarEditorHUDModel();

    private readonly UserProfile userProfile;
    private readonly WearableDictionary catalog;

    private string[] inventory = { };

    public AvatarEditorHUDController(UserProfile userProfile, WearableDictionary catalog)
    {
        view = AvatarEditorHUDView.Create(this);
        view.OnItemSelected += OnItemSelected;
        view.OnItemDeselected += OnItemDeselected;
        view.OnSkinColorChanged += OnSkinColorChanged;
        view.OnHairColorChanged += OnHairColorChanged;
        view.OnEyeColorChanged += OnEyeColorChanged;

        this.catalog = catalog;
        this.catalog.OnAdded += AddCatalogItem;
        this.catalog.OnRemoved += RemoveCatalogItem;
        ProcessCatalog(this.catalog);

        this.userProfile = userProfile;
        this.userProfile.OnUpdate += OnUserProfileUpdate;
        OnUserProfileUpdate(this.userProfile);

        SetVisibility(false);
    }

    private void ProcessCatalog(WearableDictionary wearableDictionary)
    {
        using (var iterator = wearableDictionary.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                AddCatalogItem(iterator.Current.Key, iterator.Current.Value);
            }
        }
    }

    private void OnUserProfileUpdate(UserProfile profile)
    {
        ProcessInventory(profile.inventory);
        model.avatarModel.CopyFrom(profile.avatar);
        UpdateAvatarModel();
    }

    private void ProcessInventory(string[] newInventory)
    {
        if (newInventory == null) newInventory = new string [] { };

        IEnumerable<string> removed = inventory.Where((i) => !newInventory.Contains(i));
        IEnumerable<string> added = newInventory.Where((i) => !inventory.Contains(i));

        using (IEnumerator<string> iterator = removed.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (catalog.TryGetValue(iterator.Current, out WearableItem item))
                {
                    view.RemoveWearable(item);
                }
                else
                {
                    Debug.LogError($"Item {iterator.Current} does not exist in catalog.");
                }
            }
        }

        using (IEnumerator<string> iterator = added.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (catalog.TryGetValue(iterator.Current, out WearableItem item))
                {
                    view.AddWearable(item);
                }
                else
                {
                    Debug.LogError($"Item {iterator.Current} does not exist in catalog.");
                }
            }
        }

        inventory = newInventory;
    }

    private void AddCatalogItem(string key, WearableItem value)
    {
        if (value.tags.Contains(WearableItem.baseWearableTag))
        {
            view.AddWearable(value);
        }
    }

    private void RemoveCatalogItem(string key, WearableItem value)
    {
        if (value.tags.Contains(WearableItem.baseWearableTag))
        {
            view.RemoveWearable(value);
        }
    }

    private void UpdateAvatarModel()
    {
        view.UpdateAvatarModel(model.avatarModel);
    }

    private void UpdateAvatarPreview()
    {
        view.UpdateAvatarPreview(model.avatarModel);
    }

    private void OnSkinColorChanged(Color color)
    {
        model.avatarModel.skinColor = color;
        UpdateAvatarPreview();
    }

    private void OnHairColorChanged(Color color)
    {
        model.avatarModel.hairColor = color;
        UpdateAvatarPreview();
    }

    private void OnEyeColorChanged(Color color)
    {
        model.avatarModel.eyeColor = color;
        UpdateAvatarPreview();
    }

    private void OnItemSelected(WearableItem asset)
    {
        if (asset.category == WearableItem.bodyShapeCategory)
        {
            model.avatarModel.bodyShape = asset.id;
            view.UpdateSelectedBody(model.avatarModel);
        }
        else
        {
            if (model.avatarModel.wearables.Contains(asset.id)) return;
            model.avatarModel.wearables.Add(asset.id);
        }

        UpdateAvatarPreview();
    }

    private void OnItemDeselected(string assetID)
    {
        model.avatarModel.wearables.Remove(assetID);
        UpdateAvatarPreview();
    }

    public void SaveAvatar(Texture2D faceSnapshot, Texture2D bodySnapshot)
    {
        WebInterface.SendSaveAvatar(model.avatarModel, faceSnapshot, bodySnapshot);
        userProfile.OverrideAvatar(model.avatarModel);
        userProfile.OverrideTextures(faceSnapshot, bodySnapshot);

        SetVisibility(false);
    }

    public void DiscardAndClose()
    {
        model.avatarModel.CopyFrom(userProfile.avatar);
        UpdateAvatarModel();
        UpdateAvatarPreview();
        SetVisibility(false);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        userProfile.OnUpdate -= OnUserProfileUpdate;
        catalog.OnAdded -= AddCatalogItem;
        catalog.OnRemoved -= RemoveCatalogItem;

        if (view != null)
        {
            view.OnItemSelected -= OnItemSelected;
            view.OnItemDeselected -= OnItemDeselected;
            view.OnSkinColorChanged -= OnSkinColorChanged;
            view.OnHairColorChanged -= OnHairColorChanged;
            view.OnEyeColorChanged -= OnEyeColorChanged;
            Object.Destroy(view.gameObject);
        }
    }

    public void SetConfiguration(HUDConfiguration configuration)
    {
        SetVisibility(configuration.active);
    }
}