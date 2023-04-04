using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using GPUSkinning;
using UnityEngine;

namespace AvatarSystem
{
    // [ADR 65 - https://github.com/decentraland/adr]
    public class AvatarWithHologram : Avatar
    {
        private readonly IBaseAvatar baseAvatar;

        internal AvatarWithHologram(IBaseAvatar baseAvatar, IAvatarCurator avatarCurator, ILoader loader, IAnimator animator, IVisibility visibility,
            ILOD lod, IGPUSkinning gpuSkinning, IGPUSkinningThrottlerService gpuSkinningThrottlerService, IEmoteAnimationEquipper emoteAnimationEquipper)
            : base(avatarCurator, loader, animator, visibility, lod, gpuSkinning, gpuSkinningThrottlerService, emoteAnimationEquipper)
        {
            this.baseAvatar = baseAvatar;
        }

        protected override async UniTask LoadTry(List<string> wearablesIds, List<string> emotesIds, AvatarSettings settings, CancellationToken linkedCt)
        {
            baseAvatar.FadeGhost(linkedCt).Forget(); //Avoid making the ghost fading a blocking part of the avatar
            animator.Prepare(settings.bodyshapeId, baseAvatar.GetArmatureContainer());
            List<WearableItem> emotes = await LoadWearables(wearablesIds, emotesIds, settings, baseAvatar.GetMainRenderer(), linkedCt: linkedCt);
            Prepare(settings, emotes, baseAvatar.GetArmatureContainer());
            Bind();

            MeshRenderer newCombinedRenderer = loader.combinedRenderer.GetComponent<MeshRenderer>();
            Inform(newCombinedRenderer);
            if (visibility.IsMainRenderVisible())
                await baseAvatar.Reveal(gpuSkinning.renderer, extents.y, linkedCt);
            else
                baseAvatar.RevealInstantly(gpuSkinning.renderer, extents.y);
        }

        public override void AddVisibilityConstraint(string key)
        {
            base.AddVisibilityConstraint(key);
            baseAvatar.RevealInstantly(gpuSkinning.renderer, extents.y);
        }
    }
}
