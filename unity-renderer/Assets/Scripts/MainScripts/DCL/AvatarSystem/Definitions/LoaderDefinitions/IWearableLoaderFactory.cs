namespace AvatarSystem
{
    public interface IWearableLoaderFactory
    {
        IWearableLoader GetWearableLoader(WearableItem item);
        IBodyshapeLoader GetBodyshapeLoader(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth);
    }
}