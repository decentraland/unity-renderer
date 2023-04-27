using UnityEngine;

namespace DCL.Backpack
{
    public record NftSubCategoryFilterModel
    {
        public string Name { get; set; }
        public string Filter { get; set; }
        public int ResultCount { get; set; }
        public bool ShowResultCount { get; set; }
        public Sprite Icon { get; set; }
        public bool IsSelected { get; set; }
    }
}
