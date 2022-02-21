using DCL.Builder;

namespace DCL
{
    public class DataStore_BuilderInWorld
    {
        public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
        public readonly BaseDictionary<string, CatalogItem> catalogItemDict = new BaseDictionary<string, CatalogItem>();
        public readonly BaseDictionary<string, CatalogItem> currentSceneCatalogItemDict = new BaseDictionary<string, CatalogItem>();
        public readonly BaseDictionary<string, CatalogItemPack> catalogItemPackDict = new BaseDictionary<string, CatalogItemPack>();
        public readonly BaseVariable<PublishSceneResultPayload> unpublishSceneResult = new BaseVariable<PublishSceneResultPayload>();
        public readonly BaseVariable<bool> showTaskBar = new BaseVariable<bool>();
        public readonly BaseVariable<bool> isDevBuild = new BaseVariable<bool>();
        public readonly BaseVariable<LandWithAccess[]> landsWithAccess = new BaseVariable<LandWithAccess[]>();
        public readonly BaseVariable<ProjectData[]> projectData = new BaseVariable<ProjectData[]>();
        public readonly BaseVariable<bool> areShortcutsBlocked = new BaseVariable<bool>(false);
        public readonly BaseVariable<string> lastProjectIdCreated = new BaseVariable<string>();
    }
}