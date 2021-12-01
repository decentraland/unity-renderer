using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace AvatarSystem
{
    public interface IAvatar
    {
        UniTask Load(     WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings);
    }
}