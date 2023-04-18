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
    }
}
