using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatarDummy : IBaseAvatar
    {
        private MeshReferenceHolder meshHolder;
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
            return armatureContainer;
        }

        public SkinnedMeshRenderer GetMainRenderer()
        {
            return meshHolder.meshRenderer;
        }

        public void Initialize(bool resetLoading)
        {
            if (avatarRevealerContainer == null)
                return;

            if (meshHolder != null && resetLoading)
                UnityEngine.Object.Destroy(meshHolder.gameObject);

            if (resetLoading)
                meshHolder = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("LoadingAvatarDummy"), avatarRevealerContainer).GetComponent<MeshReferenceHolder>();

            meshRenderer = meshHolder.meshRenderer;
        }

        public void FadeOut(Renderer targetRenderer) { }

    }
}
