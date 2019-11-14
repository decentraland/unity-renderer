using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using Categories = WearableLiterals.Categories;

public class AvatarEditorHUDController : IDisposable, IHUD
{
    protected static readonly string[] categoriesThatMustHaveSelection = { Categories.BODY_SHAPE, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH };
    protected static readonly string[] categoriesToRandomize = { Categories.HAIR, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH, Categories.FACIAL, Categories.HAIR, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET };

    private UserProfile userProfile;
    private WearableDictionary catalog;
    private readonly Dictionary<string, List<WearableItem>> wearablesByCategory = new Dictionary<string, List<WearableItem>>();
    protected readonly AvatarEditorHUDModel model = new AvatarEditorHUDModel();

    private ColorList skinColorList;
    private ColorList eyeColorList;
    private ColorList hairColorList;

    protected AvatarEditorHUDView view;

    public AvatarEditorHUDController(UserProfile userProfile, WearableDictionary catalog)
    {
        view = AvatarEditorHUDView.Create(this);

        skinColorList = Resources.Load<ColorList>("SkinTone");
        hairColorList = Resources.Load<ColorList>("HairColor");
        eyeColorList = Resources.Load<ColorList>("EyeColor");
        view.SetColors(skinColorList.colors, hairColorList.colors, eyeColorList.colors);

        this.catalog = catalog;
        ProcessCatalog(this.catalog);
        this.catalog.OnAdded += AddWearable;
        this.catalog.OnRemoved += RemoveWearable;

        this.userProfile = userProfile;
        LoadUserProfile(userProfile);
        this.userProfile.OnUpdate += LoadUserProfile;
    }

    private void LoadUserProfile(UserProfile userProfile)
    {
        if (userProfile?.avatar == null || string.IsNullOrEmpty(userProfile.avatar.bodyShape)) return;

        var bodyShape = CatalogController.wearableCatalog.Get(userProfile.avatar.bodyShape);
        if (bodyShape == null)
        {
            return;
        }

        ProcessCatalog(this.catalog);
        EquipBodyShape(bodyShape);
        EquipSkinColor(userProfile.avatar.skinColor);
        EquipHairColor(userProfile.avatar.hairColor);
        EquipEyesColor(userProfile.avatar.eyeColor);

        model.wearables.Clear();
        view.UnselectAllWearables();
        int wearablesCount = userProfile.avatar.wearables.Count;
        for (var i = 0; i < wearablesCount; i++)
        {
            var wearable = CatalogController.wearableCatalog.Get(userProfile.avatar.wearables[i]);
            if (wearable == null)
            {
                Debug.LogError($"Couldn't find wearable with ID {userProfile.avatar.wearables[i]}");
                continue;
            }
            EquipWearable(wearable);
        }
        EnsureWearablesCategoriesNotEmpty();

        view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    private void EnsureWearablesCategoriesNotEmpty()
    {
        var categoriesInUse = model.wearables.Select(x => x.category).ToArray();
        for (var i = 0; i < categoriesThatMustHaveSelection.Length; i++)
        {
            var category = categoriesThatMustHaveSelection[i];
            if (category != Categories.BODY_SHAPE && !(categoriesInUse.Contains(category)))
            {
                WearableItem wearable;
                var defaultItemId = WearableLiterals.DefaultWearables.GetDefaultWearable(model.bodyShape.id, category);
                if (defaultItemId != null)
                {
                    wearable = CatalogController.wearableCatalog.Get(defaultItemId);
                }
                else
                {
                    wearable = wearablesByCategory[category].FirstOrDefault(x => x.SupportsBodyShape(model.bodyShape.id));
                }

                if (wearable != null)
                {
                    EquipWearable(wearable);
                }
            }
        }
    }

    public void WearableClicked(string wearableId)
    {
        var wearable = CatalogController.wearableCatalog.Get(wearableId);
        if (wearable == null) return;

        if (wearable.category == Categories.BODY_SHAPE)
        {
            EquipBodyShape(wearable);
        }
        else
        {
            if (model.wearables.Contains(wearable))
            {
                if (!categoriesThatMustHaveSelection.Contains(wearable.category))
                {
                    UnequipWearable(wearable);
                }
            }
            else
            {
                var sameCategoryEquipped = model.wearables.FirstOrDefault(x => x.category == wearable.category);
                if (sameCategoryEquipped != null)
                {
                    UnequipWearable(sameCategoryEquipped);
                }
                EquipWearable(wearable);
            }
        }
        view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    public void HairColorClicked(Color color)
    {
        EquipHairColor(color);
        view.SelectHairColor(model.hairColor);
        view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    public void SkinColorClicked(Color color)
    {
        EquipSkinColor(color);
        view.SelectSkinColor(model.skinColor);
        view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    public void EyesColorClicked(Color color)
    {
        EquipEyesColor(color);
        view.SelectEyeColor(model.eyesColor);
        view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    private void EquipHairColor(Color color)
    {
        if (hairColorList.colors.Any(x => x.AproxComparison(color)))
        {
            model.hairColor = color;
            view.SelectHairColor(model.hairColor);
        }
    }

    private void EquipEyesColor(Color color)
    {
        if (eyeColorList.colors.Any(x => x.AproxComparison(color)))
        {
            model.eyesColor = color;
            view.SelectEyeColor(model.eyesColor);
        }
    }

    private void EquipSkinColor(Color color)
    {
        if (skinColorList.colors.Any(x => x.AproxComparison(color)))
        {
            model.skinColor = color;
            view.SelectSkinColor(model.skinColor);
        }
    }

    private void EquipBodyShape(WearableItem bodyShape)
    {
        if (bodyShape.category != Categories.BODY_SHAPE)
        {
            Debug.LogError($"Item ({bodyShape.id} is not a body shape");
            return;
        }
        if (model.bodyShape == bodyShape) return;

        model.bodyShape = bodyShape;
        view.UpdateSelectedBody(bodyShape);

        int wearablesCount = model.wearables.Count;
        for (var i = wearablesCount - 1; i >= 0; i--)
        {
            UnequipWearable(model.wearables[i]);
        }

        var defaultWearables = WearableLiterals.DefaultWearables.GetDefaultWearables(bodyShape.id);
        for (var i = 0; i < defaultWearables.Length; i++)
        {
            EquipWearable(catalog.Get(defaultWearables[i]));
        }
    }

    private void EquipWearable(WearableItem wearable)
    {
        if (!wearablesByCategory.ContainsKey(wearable.category)) return;

        if (wearablesByCategory[wearable.category].Contains(wearable) && wearable.SupportsBodyShape(model.bodyShape.id) && !model.wearables.Contains(wearable))
        {
            var toReplace = GetWearablesReplacedBy(wearable);
            toReplace.ForEach(UnequipWearable);
            model.wearables.Add(wearable);
            view.SelectWearable(wearable);
        }
    }

    private void UnequipWearable(WearableItem wearable)
    {
        if (model.wearables.Contains(wearable))
        {
            model.wearables.Remove(wearable);
            view.UnselectWearable(wearable);
        }
    }

    private void ProcessCatalog(WearableDictionary catalog)
    {
        wearablesByCategory.Clear();
        using (var iterator = catalog.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                AddWearable(iterator.Current.Key, iterator.Current.Value);
            }
        }
    }

    private void AddWearable(string id, WearableItem wearable)
    {
        if (!wearable.tags.Contains("base-wearable") && !userProfile.ContainsItem(id))
        {
            return;
        }

        if (!wearablesByCategory.ContainsKey(wearable.category))
        {
            wearablesByCategory.Add(wearable.category, new List<WearableItem>());
        }
        wearablesByCategory[wearable.category].Add(wearable);
        view.AddWearable(wearable);
    }

    private void RemoveWearable(string id, WearableItem wearable)
    {
        if (wearablesByCategory.ContainsKey(wearable.category))
        {
            if (wearablesByCategory[wearable.category].Remove(wearable))
            {
                if (wearablesByCategory[wearable.category].Count == 0)
                {
                    wearablesByCategory.Remove(wearable.category);
                }
            }
        }
        view.RemoveWearable(wearable);
    }

    public void RandomizeWearables()
    {
        EquipHairColor(hairColorList.colors[UnityEngine.Random.Range(0, hairColorList.colors.Count)]);
        EquipEyesColor(eyeColorList.colors[UnityEngine.Random.Range(0, eyeColorList.colors.Count)]);

        model.wearables.Clear();
        view.UnselectAllWearables();
        using (var iterator = wearablesByCategory.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                string category = iterator.Current.Key;
                if (!categoriesToRandomize.Contains(category))
                {
                    continue;
                }

                var supportedWearables = iterator.Current.Value.Where(x => x.SupportsBodyShape(model.bodyShape.id)).ToArray();
                if (supportedWearables.Length == 0)
                {
                    Debug.LogError($"Couldn't get any wearable for category {category} and bodyshape {model.bodyShape.id}");
                }
                var wearable = supportedWearables[UnityEngine.Random.Range(0, supportedWearables.Length - 1)];
                EquipWearable(wearable);
            }
        }
        view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    public List<WearableItem> GetWearablesReplacedBy(WearableItem wearableItem)
    {
        List<WearableItem> wearablesToReplace = new List<WearableItem>();

        HashSet<string> categoriesToReplace = new HashSet<string>(wearableItem.GetReplacesList(model.bodyShape.id) ?? new string[0]);

        int wearableCount = model.wearables.Count;
        for (int i = 0; i < wearableCount; i++)
        {
            var wearable = model.wearables[i];
            if (wearable == null) continue;

            if (categoriesToReplace.Contains(wearable.category))
            {
                wearablesToReplace.Add(wearable);
            }
            else
            {
                //For retrocompatibility's sake we check current wearables against new one (compatibility matrix is symmetrical)
                HashSet<string> replacesList = new HashSet<string>(wearable.GetReplacesList(model.bodyShape.id) ?? new string[0]);
                if (replacesList.Contains(wearableItem.category))
                {
                    wearablesToReplace.Add(wearable);
                }
            }
        }

        return wearablesToReplace;
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        CleanUp();
    }

    public void CleanUp()
    {
        view?.CleanUp();
        this.userProfile.OnUpdate -= LoadUserProfile;
        this.catalog.OnAdded -= AddWearable;
        this.catalog.OnRemoved -= RemoveWearable;
    }

    public void SetConfiguration(HUDConfiguration configuration)
    {
        SetVisibility(configuration.active);
    }

    public void SaveAvatar(Texture2D faceSnapshot, Texture2D bodySnapshot)
    {
        var avatarModel = model.ToAvatarModel();
        WebInterface.SendSaveAvatar(avatarModel, faceSnapshot, bodySnapshot);
        userProfile.OverrideAvatar(avatarModel, faceSnapshot, bodySnapshot);

        SetVisibility(false);
    }

    public void DiscardAndClose()
    {
        LoadUserProfile(userProfile);
        SetVisibility(false);
    }
}