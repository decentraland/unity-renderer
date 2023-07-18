namespace AvatarSystem
{
    public interface IWearableLoaderFactory
    {
        IWearableLoader GetWearableLoader(WearableItem item);
        IBodyshapeLoader GetBodyShapeLoader(BodyWearables bodyWearables);
    }
}
