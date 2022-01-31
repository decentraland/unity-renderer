namespace AvatarSystem
{
    public interface IRetrieverFactory
    {
        IWearableRetriever GetWearableRetriever();
        IFacialFeatureRetriever GetFacialFeatureRetriever();
    }
}