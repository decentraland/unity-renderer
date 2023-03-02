namespace DCL
{
    public class DataStore_FeatureFlag
    {
        public readonly BaseVariable<FeatureFlag> flags = new BaseVariable<FeatureFlag>(new FeatureFlag());
    }
}
