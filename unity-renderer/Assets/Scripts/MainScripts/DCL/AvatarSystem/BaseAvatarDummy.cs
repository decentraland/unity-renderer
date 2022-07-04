using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatarDummy : IBaseAvatar
    {
        public IBaseAvatarRevealer avatarRevealer { get; set; }
        private Transform avatarRevealerContainer;
        public GameObject armatureContainer;
        public SkinnedMeshRenderer meshRenderer { get; private set; }

        public BaseAvatarDummy(Transform avatarRevealerContainer, GameObject armatureContainer)
        {
            this.avatarRevealerContainer = avatarRevealerContainer;
            this.armatureContainer = armatureContainer;
        }

        public GameObject GetArmatureContainer()
        {
            return null;
        }

        public SkinnedMeshRenderer GetMainRenderer()
        {
            return null;
        }

        public void Initialize()
        {
            //if (meshRenderer == null)
            //    meshRenderer = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("LoadingAvatarDummy"), avatarRevealerContainer).GetComponentInChildren<SkinnedMeshRenderer>();
        }

        public void FadeOut(MeshRenderer targetRenderer, bool playParticles) { }

    }
}
