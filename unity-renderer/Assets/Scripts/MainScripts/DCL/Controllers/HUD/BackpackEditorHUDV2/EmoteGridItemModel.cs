namespace DCL.Backpack
{
    public record EmoteGridItemModel
    {
        public string EmoteId { get; set; }
        public bool IsLoading { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEquipped { get; set; }
        public string ImageUrl { get; set; }
        public bool IsNew { get; set; }
    }
}
