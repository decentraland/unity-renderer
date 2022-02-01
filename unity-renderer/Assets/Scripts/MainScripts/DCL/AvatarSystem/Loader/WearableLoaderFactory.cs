using System;
using UnityEngine.Assertions;

namespace AvatarSystem
{
    public class WearableLoaderFactory : IWearableLoaderFactory
    {
        public IWearableLoader GetWearableLoader(WearableItem item)
        {
            if (item == null)
                throw new Exception("Requested a WearableLoader with a null WearableItem");

            if (item.data.category == WearableLiterals.Categories.BODY_SHAPE ||
                item.data.category == WearableLiterals.Categories.EYES ||
                item.data.category == WearableLiterals.Categories.EYEBROWS ||
                item.data.category == WearableLiterals.Categories.MOUTH)
                throw new Exception("Requested a WearableLoader with a bodyshape or facial feature");

            return new WearableLoader( new WearableRetriever(), item);
        }

        public IBodyshapeLoader GetBodyshapeLoader(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth)
        {
            if (bodyshape == null)
                throw new Exception("Requested a BodyshapeLoader with a null Bodyshape");

            if (eyes == null)
                throw new Exception("Requested a BodyshapeLoader with null eyes");

            if (eyebrows == null)
                throw new Exception("Requested a BodyshapeLoader with null eyebrows");

            if (mouth == null)
                throw new Exception("Requested a BodyshapeLoader with null mouth");

            if (bodyshape.data.category != WearableLiterals.Categories.BODY_SHAPE)
                throw new Exception($"Bodyshape's category is not {WearableLiterals.Categories.BODY_SHAPE}");

            if (eyes.data.category != WearableLiterals.Categories.EYES)
                throw new Exception($"Eye's category is not {WearableLiterals.Categories.EYES}");

            if (eyebrows.data.category != WearableLiterals.Categories.EYEBROWS)
                throw new Exception($"Eyebrows's category is not {WearableLiterals.Categories.EYEBROWS}");

            if (mouth.data.category != WearableLiterals.Categories.MOUTH)
                throw new Exception($"Mouth's category is not {WearableLiterals.Categories.MOUTH}");

            return new BodyShapeLoader(new RetrieverFactory(), bodyshape,  eyes,  eyebrows,  mouth);
        }
    }
}