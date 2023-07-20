namespace AvatarSystem
{
    public class BodyWearables
    {
        public WearableItem BodyShape { get; private set; }
        public WearableItem Eyes{ get; private set; }
        public WearableItem Eyebrows{ get; private set; }
        public WearableItem Mouth{ get; private set; }

        public BodyWearables(WearableItem bodyShape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth)
        {
            this.BodyShape = bodyShape;
            this.Eyes = eyes;
            this.Eyebrows = eyebrows;
            this.Mouth = mouth;
        }
    }
}
