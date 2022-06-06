using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatar : IBaseAvatar
    {
        private AvatarReveal avatarRevealer;
        private Transform avatarRevealerContainer;
        public GameObject armatureContainer;
        public SkinnedMeshRenderer meshRenderer { get; private set; }

        public BaseAvatar(Transform avatarRevealerContainer, GameObject armatureContainer) 
        {
            this.avatarRevealerContainer = avatarRevealerContainer;
            this.armatureContainer = armatureContainer;
        }

        public GameObject GetArmatureContainer()
        {
            return armatureContainer;
        }

        public SkinnedMeshRenderer GetMainRenderer()
        {
            return avatarRevealer.meshRenderer;
        }

        public void Initialize(bool resetLoading) 
        {
            if (avatarRevealerContainer == null)
                return;

            if (avatarRevealer == null)
                avatarRevealer = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("LoadingAvatar"), avatarRevealerContainer).GetComponent<AvatarReveal>();

            meshRenderer = avatarRevealer.meshRenderer;
        }

        public void FadeOut(MeshRenderer targetRenderer) 
        {
            if (avatarRevealerContainer == null) 
                return;

            avatarRevealer.AddTarget(targetRenderer);
            avatarRevealer.avatarLoaded = true;
        }

    }
}
