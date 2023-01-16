namespace DCL
{
    public class DataStore_FeatureFlag
    {
        public readonly string DECOUPLED_LOADING_SCREEN_FF = "decoupled_loading_screen";

        public readonly BaseVariable<FeatureFlag> flags = new BaseVariable<FeatureFlag>(new FeatureFlag());
    }
}
