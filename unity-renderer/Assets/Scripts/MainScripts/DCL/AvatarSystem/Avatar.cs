using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace AvatarSystem
{
    public class Avatar : IAvatar
    {
        private readonly ILoader loader;

        public Avatar(ILoader loader) { this.loader = loader; }

        public async UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings) { await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings); }
    }
}