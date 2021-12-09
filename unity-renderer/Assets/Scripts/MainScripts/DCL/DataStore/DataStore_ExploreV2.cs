namespace DCL
{
    public class DataStore_ExploreV2
    {
        public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isOpen = new BaseVariable<bool>(false);
    }
}