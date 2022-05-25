using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatar : IBaseAvatar
    {
        private AvatarReveal avatarRevealer;
        private Transform avatarRevealerContainer;

        public BaseAvatar(Transform avatarRevealerContainer) 
        {
            this.avatarRevealerContainer = avatarRevealerContainer;
        }

        public void Initialize(bool resetLoading) 
        {
            if (avatarRevealerContainer == null)
                return;

            if (avatarRevealer != null && resetLoading)
                UnityEngine.Object.Destroy(avatarRevealer.gameObject);

            if(resetLoading)
                avatarRevealer = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("LoadingAvatar"), avatarRevealerContainer).GetComponent<AvatarReveal>();
        }

        public void FadeIn() 
        {
        }

        public void FadeOut(Renderer targetRenderer) 
        {
            if (avatarRevealerContainer == null) 
                return;

            avatarRevealer.AddTarget(targetRenderer);
            avatarRevealer.avatarLoaded = true;
            Debug.Log("Completed fade out");
        }

    }
}
