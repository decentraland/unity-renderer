using DCL;
using GPUSkinning;
using UnityEngine;

namespace AvatarSystem
{
    public class AvatarFactory : IAvatarFactory
    {
        private readonly ServiceLocator serviceLocator;

        public AvatarFactory(ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public Avatar CreateAvatar(GameObject avatarContainer, IAnimator animator, ILOD lod, IVisibility visibility) =>
            new (
                CreateAvatarCurator(),
                CreateLoader(avatarContainer),
                animator,
                visibility,
                lod,
                new SimpleGPUSkinning(),
                new GPUSkinningThrottler(),
                new EmoteAnimationEquipper(animator, DataStore.i.emotes)
            );

        public AvatarWithHologram CreateAvatarWithHologram(
            GameObject avatarContainer,
            Transform avatarRevealContainer,
            GameObject armatureContainer,
            IAnimator animator,
            ILOD lod,
            IVisibility visibility
        ) =>
            new (
                new BaseAvatar(avatarRevealContainer, armatureContainer, lod),
                CreateAvatarCurator(),
                CreateLoader(avatarContainer),
                animator,
                visibility,
                lod,
                new SimpleGPUSkinning(),
                new GPUSkinningThrottler(),
                new EmoteAnimationEquipper(animator, DataStore.i.emotes)
            );

        private Loader CreateLoader(GameObject avatarContainer) =>
            new (new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper());

        private AvatarCurator CreateAvatarCurator() =>
            new (new WearableItemResolver(), serviceLocator.Get<IEmotesCatalogService>());
    }
}
