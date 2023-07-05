using System;
using DCL;
using UnityEngine.Assertions;

namespace AvatarSystem
{
    public class WearableLoaderFactory : IWearableLoaderFactory
    {
        public IWearableLoader GetWearableLoader(WearableItem item)
        {
            if (item == null)
                throw new Exception("Requested a WearableLoader with a null WearableItem");

            if (item.data.category
                is WearableLiterals.Categories.BODY_SHAPE
                or WearableLiterals.Categories.EYES
                or WearableLiterals.Categories.EYEBROWS
                or WearableLiterals.Categories.MOUTH)
                throw new Exception("Requested a WearableLoader with a bodyshape or facial feature");

            return new WearableLoader(new WearableRetriever(), item);
        }

        public IBodyshapeLoader GetBodyShapeLoader(BodyWearables bodyWearables)
        {
            if (bodyWearables.bodyshape == null)
                throw new Exception("Requested a BodyshapeLoader with a null Bodyshape");

            if (bodyWearables.bodyshape.data.category != WearableLiterals.Categories.BODY_SHAPE)
                throw new Exception($"Bodyshape's category is not {WearableLiterals.Categories.BODY_SHAPE}");

            if (bodyWearables.eyes != null && bodyWearables.eyes.data.category != WearableLiterals.Categories.EYES)
                throw new Exception($"Eye's category is not {WearableLiterals.Categories.EYES}");

            if (bodyWearables.eyebrows != null && bodyWearables.eyebrows.data.category != WearableLiterals.Categories.EYEBROWS)
                throw new Exception($"Eyebrows's category is not {WearableLiterals.Categories.EYEBROWS}");

            if (bodyWearables.mouth != null && bodyWearables.mouth.data.category != WearableLiterals.Categories.MOUTH)
                throw new Exception($"Mouth's category is not {WearableLiterals.Categories.MOUTH}");

            return new BodyShapeLoader(new RetrieverFactory(), bodyWearables);
        }
    }
}
