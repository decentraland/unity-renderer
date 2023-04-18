using DCL;
using DCLServices.WearablesCatalogService;
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

        public IAvatar CreateAvatar(GameObject avatarContainer, IAnimator animator, ILOD lod, IVisibility visibility) =>
            new Avatar(
                CreateAvatarCurator(),
                CreateLoader(avatarContainer),
                animator,
                visibility,
                lod,
                new SimpleGPUSkinning(),
                serviceLocator.Get<IGPUSkinningThrottlerService>(),
                new EmoteAnimationEquipper(animator, DataStore.i.emotes)
            );

        public IAvatar CreateAvatarWithHologram(
            GameObject avatarContainer,
            IBaseAvatar baseAvatar,
            IAnimator animator,
            ILOD lod,
            IVisibility visibility
        ) =>
            new AvatarWithHologram(
                baseAvatar,
                CreateAvatarCurator(),
                CreateLoader(avatarContainer),
                animator,
                visibility,
                lod,
                new SimpleGPUSkinning(),
                serviceLocator.Get<IGPUSkinningThrottlerService>(),
                new EmoteAnimationEquipper(animator, DataStore.i.emotes)
            );

        private Loader CreateLoader(GameObject avatarContainer) =>
            new (new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper());

        private AvatarCurator CreateAvatarCurator() =>
            new (new WearableItemResolver(serviceLocator.Get<IWearablesCatalogService>()), serviceLocator.Get<IEmotesCatalogService>());
    }
}
