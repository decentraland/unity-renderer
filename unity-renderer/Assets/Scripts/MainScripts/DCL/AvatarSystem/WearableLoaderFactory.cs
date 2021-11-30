using AvatarSystem;

public class WearableLoaderFactory : IWearableLoaderFactory
{
    public IWearableLoader GetWearableLoader(WearableItem item) { return new WearableLoader( new WearableRetriever(), item); }
    public IBodyshapeLoader GetBodyshapeLoader(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth) { return new BodyShapeLoader(new RetrieverFactory(), bodyshape,  eyes,  eyebrows,  mouth); }
}

public class RetrieverFactory : IRetrieverFactory
{
    public IWearableRetriever GetWearableRetriever() => new WearableRetriever();
    public IFacialFeatureRetriever GetFacialFeatureRetriever() => new FacialFeatureRetriever();
}