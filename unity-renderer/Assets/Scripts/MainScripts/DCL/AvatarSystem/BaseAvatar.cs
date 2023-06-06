using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

namespace AvatarSystem
{
    public class BaseAvatar : IBaseAvatar
    {
        internal static readonly int REVEAL_POSITION_ID = Shader.PropertyToID("_RevealPosition");
        internal static readonly int REVEAL_NORMAL_ID = Shader.PropertyToID("_RevealNormal");
        internal static readonly int COLOR_ID = Shader.PropertyToID("_Color");

        internal readonly IBaseAvatarReferences baseAvatarReferences;
        internal readonly List<Material> cachedMaterials = new ();
        internal readonly Material ghostMaterial;

        public SkinnedMeshRenderer SkinnedMeshRenderer => baseAvatarReferences.SkinnedMeshRenderer;
        public GameObject ArmatureContainer => baseAvatarReferences.ArmatureContainer.gameObject;

        private CancellationTokenSource revealCts = new ();
        private CancellationTokenSource fadeInGhostCts = new ();

        public BaseAvatar(IBaseAvatarReferences baseAvatarReferences)
        {
            Assert.IsNotNull(baseAvatarReferences.SkinnedMeshRenderer);

            this.baseAvatarReferences = baseAvatarReferences;
            ghostMaterial = baseAvatarReferences.SkinnedMeshRenderer.material;
            ghostMaterial.SetColor(COLOR_ID, Utils.GetRandomColorInGradient(baseAvatarReferences.GhostMinColor, this.baseAvatarReferences.GhostMaxColor));
        }

        public async UniTask FadeGhost(CancellationToken cancellationToken = default)
        {
            fadeInGhostCts?.Cancel();
            fadeInGhostCts?.Dispose();
            fadeInGhostCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(fadeInGhostCts.Token, cancellationToken);

            //Reset revealing position for ghots material
            ghostMaterial.SetVector(REVEAL_NORMAL_ID, Vector3.up * -1);
            ghostMaterial.SetVector(REVEAL_POSITION_ID, Vector3.zero);

            await ghostMaterial
                 .DOFade(1, COLOR_ID, baseAvatarReferences.FadeGhostSpeed)
                 .SetSpeedBased(true)
                 .ToUniTaskInstantCancelation(cancellationToken: linkedCts.Token);
        }

        public async UniTask Reveal(Renderer targetRenderer, float avatarHeight, float completionHeight, CancellationToken cancellationToken = default)
        {
            revealCts?.Cancel();
            revealCts?.Dispose();
            revealCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(revealCts.Token, cancellationToken);

            // keep a list to gather materials to avoid allocations
            cachedMaterials.Clear();
            targetRenderer?.GetMaterials(cachedMaterials);

            UniTask GetRevealTask(Material material, float revealPosition, float completionPosition)
            {
                material.SetVector(REVEAL_NORMAL_ID, Vector3.up * -1);
                material.SetVector(REVEAL_POSITION_ID, Vector3.zero);

                return material.DOVector(Vector3.up * revealPosition, REVEAL_POSITION_ID, baseAvatarReferences.RevealSpeed)
                               .SetSpeedBased(true)
                               .OnComplete(() =>
                                {
                                    baseAvatarReferences.ParticlesContainer.SetActive(false);
                                    SetRevealPosition(material, completionPosition);
                                })
                               .ToUniTaskInstantCancelation(true, cancellationToken: linkedCts.Token);
            }

            try
            {
                baseAvatarReferences.ParticlesContainer.SetActive(true);
                List<UniTask> tasks = new List<UniTask>();
                tasks.Add(GetRevealTask(ghostMaterial, avatarHeight, completionHeight));

                for (var index = 0; index < cachedMaterials.Count; index++) { tasks.Add(GetRevealTask(cachedMaterials[index], -avatarHeight, -completionHeight)); }

                await UniTask.WhenAll(tasks);
            }
            finally
            {
                baseAvatarReferences.ParticlesContainer.SetActive(false);
                FadeOutGhostMaterial();
            }
        }

        public void RevealInstantly(Renderer targetRenderer, float avatarHeight)
        {
            revealCts?.Cancel();
            revealCts?.Dispose();
            revealCts = null;

            cachedMaterials.Clear();
            if(targetRenderer != null)
                targetRenderer.GetMaterials(cachedMaterials);

            ghostMaterial.SetVector(REVEAL_NORMAL_ID, Vector3.up * -1);
            SetRevealPosition(ghostMaterial, avatarHeight);

            for (var i = 0; i < cachedMaterials.Count; i++)
            {
                cachedMaterials[i].SetVector(REVEAL_NORMAL_ID, Vector3.up * -1);
                SetRevealPosition(cachedMaterials[i], -avatarHeight);
            }
            baseAvatarReferences.ParticlesContainer.SetActive(false);
            FadeOutGhostMaterial();
        }

        private void FadeOutGhostMaterial()
        {
            Color color = ghostMaterial.GetColor(COLOR_ID);
            color.a = 0;
            ghostMaterial.color = color;
        }

        internal static void SetRevealPosition(Material material, float height)
        {
            material.SetVector(REVEAL_POSITION_ID, Vector3.up * height);
        }

        public void Dispose()
        {
            revealCts?.Cancel();
            revealCts?.Dispose();
            revealCts = null;
            fadeInGhostCts?.Cancel();
            fadeInGhostCts?.Dispose();
            revealCts = fadeInGhostCts;
        }
    }
}
