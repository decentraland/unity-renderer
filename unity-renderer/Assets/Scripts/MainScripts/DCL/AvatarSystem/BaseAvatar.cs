using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatar : IBaseAvatar
    {
        public IBaseAvatarRevealer avatarRevealer { get; set; }
        private ILOD lod;
        private Transform avatarRevealerContainer;
        private CancellationTokenSource transitionCts = new CancellationTokenSource();
        
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

        public async UniTask FadeOut(MeshRenderer targetRenderer, bool withTransition, CancellationToken cancellationToken) 
        {
            if (avatarRevealerContainer == null) 
                return;
            
            transitionCts ??= new CancellationTokenSource();
            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, transitionCts.Token).Token;
            linkedCt.ThrowIfCancellationRequested();
            
            avatarRevealer.AddTarget(targetRenderer);
            //If canceled, the final state of the avatar is handle inside StartAvatarRevealAnimation
            await avatarRevealer.StartAvatarRevealAnimation(withTransition, linkedCt);
            
            transitionCts?.Dispose();
            transitionCts = null;
        }
        
        public void CancelTransition()
        {
            transitionCts?.Cancel();
            transitionCts?.Dispose();
            transitionCts = null;
        }

    }
}
