using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatar : IBaseAvatar
    {
        public IBaseAvatarRevealer avatarRevealer { get; set; }
        private ILOD lod;
        private Transform avatarRevealerContainer;
        public GameObject armatureContainer;
        public SkinnedMeshRenderer meshRenderer { get; private set; }

        public BaseAvatar(Transform avatarRevealerContainer, GameObject armatureContainer, ILOD lod) 
        {
            this.avatarRevealerContainer = avatarRevealerContainer;
            this.armatureContainer = armatureContainer;
            this.lod = lod;
        }

        public GameObject GetArmatureContainer()
        {
            return armatureContainer;
        }

        public SkinnedMeshRenderer GetMainRenderer()
        {
            return avatarRevealer.GetMainRenderer();
        }

        public void Initialize() 
        {
            if (avatarRevealer == null)
            {
                avatarRevealer = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("LoadingAvatar"), avatarRevealerContainer).GetComponent<BaseAvatarReveal>();
                avatarRevealer.InjectLodSystem(lod);
            }
            else
            {
                avatarRevealer.Reset();
            }

            meshRenderer = avatarRevealer.GetMainRenderer();
        }

        public void FadeOut(MeshRenderer targetRenderer, bool playParticles) 
        {
            if (avatarRevealerContainer == null) 
                return;

            avatarRevealer.AddTarget(targetRenderer);
            avatarRevealer.StartAvatarRevealAnimation(playParticles);
        }

    }
}
