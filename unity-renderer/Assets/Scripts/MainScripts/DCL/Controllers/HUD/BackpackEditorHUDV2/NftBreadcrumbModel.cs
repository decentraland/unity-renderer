namespace DCL.Backpack
{
    public record NftBreadcrumbModel
    {
        public int Current { get; set; }
        public (string Filter, string Name)[] Path { get; set; }
        public int ResultCount { get; set; }
    }
}
