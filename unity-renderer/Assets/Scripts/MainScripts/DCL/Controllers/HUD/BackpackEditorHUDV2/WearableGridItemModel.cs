using DCLServices.WearablesCatalogService;
using System;

namespace DCL.Backpack
{
    public record WearableGridItemModel
    {
        public string WearableId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEquipped { get; set; }
        public string ImageUrl { get; set; }
        public NftRarity Rarity { get; set; }
        public bool IsNew { get; set; }
        public string Category { get; set; }
        public bool UnEquipAllowed { get; set; } = true;
        public bool IsCompatibleWithBodyShape { get; set; } = true;
        public bool IsSmartWearable { get; set; } = false;

        public virtual bool Equals(WearableGridItemModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            return WearableId == other.WearableId
                   && IsSelected == other.IsSelected
                   && IsEquipped == other.IsEquipped
                   && ImageUrl == other.ImageUrl
                   && Rarity == other.Rarity
                   && IsNew == other.IsNew
                   && Category == other.Category
                   && UnEquipAllowed == other.UnEquipAllowed
                   && IsCompatibleWithBodyShape == other.IsCompatibleWithBodyShape
                   && IsSmartWearable == other.IsSmartWearable;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(WearableId);
            hashCode.Add(IsSelected);
            hashCode.Add(IsEquipped);
            hashCode.Add(ImageUrl);
            hashCode.Add((int) Rarity);
            hashCode.Add(IsNew);
            hashCode.Add(Category);
            hashCode.Add(UnEquipAllowed);
            hashCode.Add(IsCompatibleWithBodyShape);
            hashCode.Add(IsSmartWearable);
            return hashCode.ToHashCode();
        }
    }
}
