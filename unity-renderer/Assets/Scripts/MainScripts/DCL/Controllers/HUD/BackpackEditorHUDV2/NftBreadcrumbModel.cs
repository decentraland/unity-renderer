namespace DCL.Backpack
{
    public record NftBreadcrumbModel
    {
        public int Current { get; set; }
        public (string Filter, string Name, string Type, bool Removable)[] Path { get; set; }
        public int ResultCount { get; set; }
    }
}
