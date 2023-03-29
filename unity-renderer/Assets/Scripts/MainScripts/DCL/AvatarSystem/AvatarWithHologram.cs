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
            baseAvatar.Initialize();

            List<WearableItem> emotes = await LoadWearables(wearablesIds, emotesIds, settings, baseAvatar.GetMainRenderer(), linkedCt: linkedCt);
            Prepare(settings, emotes, baseAvatar.GetArmatureContainer());
            Bind();

            MeshRenderer newCombinedRenderer = loader.combinedRenderer.GetComponent<MeshRenderer>();
            Inform(newCombinedRenderer);

            await baseAvatar.FadeOut(newCombinedRenderer, visibility.IsMainRenderVisible(), linkedCt);
        }

        public override void AddVisibilityConstraint(string key)
        {
            base.AddVisibilityConstraint(key);
            baseAvatar.CancelTransition();
        }
    }
}
