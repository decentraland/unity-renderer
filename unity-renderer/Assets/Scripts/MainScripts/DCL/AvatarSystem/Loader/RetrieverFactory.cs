namespace AvatarSystem
{
    public class RetrieverFactory : IRetrieverFactory
    {
        public IWearableRetriever GetWearableRetriever() => new WearableRetriever();
        public IFacialFeatureRetriever GetFacialFeatureRetriever() => new FacialFeatureRetriever();
    }
}