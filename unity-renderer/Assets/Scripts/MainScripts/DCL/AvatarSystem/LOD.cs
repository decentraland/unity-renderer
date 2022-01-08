using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvatarSystem
{
    public class LOD : ILOD
    {
        private const float TRANSITION_DURATION = 0.75f;

        /// <summary>
        /// 0 = 3D Avatar, SSAO, Facial Features
        /// 1 = 3D Avatar, NO SSAO, NO Facial Features
        /// 2 = Impostor, NO 3D Avatar
        /// </summary>
        public int lodIndex { get; private set; } = -1;

        private readonly GameObject impostorContainer;

        private IEnumerable<Renderer> facialFeatures;
        private Renderer combinedAvatar;
        private readonly Renderer impostorRenderer;
        private readonly MeshFilter impostorMeshFilter;
        private float avatarAlpha;

        private CancellationTokenSource transitionCTS;

        public LOD(GameObject impostorContainer)
        {
            this.impostorContainer = impostorContainer;
            impostorRenderer = CreateImpostor();
            impostorRenderer.enabled = false;

            impostorMeshFilter = impostorRenderer.GetComponent<MeshFilter>();
            SetImpostorTexture(null);
        }

        /// <summary>
        /// Set the impostor texture (null will take a randomized one)
        /// </summary>
        /// <param name="texture"></param>
        public void SetImpostorTexture(Texture2D texture)
        {
            if (texture == null)
                AvatarRendererHelpers.RandomizeAndApplyGenericImpostor(impostorMeshFilter.mesh, impostorRenderer.material);
            else
                AvatarRendererHelpers.SetImpostorTexture(texture, impostorMeshFilter.mesh, impostorRenderer.material);
        }

        public void SetImpostorTint(Color color) { AvatarRendererHelpers.SetImpostorTintColor(impostorRenderer.material, color); }

        public void Bind(Renderer combinedAvatar, IEnumerable<Renderer> facialFeatures)
        {
            this.combinedAvatar = combinedAvatar;
            this.facialFeatures = facialFeatures;
        }

        public void SetLodIndex(int lodIndex, bool inmediate = false)
        {
            if (lodIndex == this.lodIndex)
                return;

            if (inmediate)
            {
                avatarAlpha = lodIndex <= 1 ? 1f : 0f;
                UpdateSSAO(lodIndex);
                UpdateFacialFeatures(lodIndex);
                UpdateAlpha(avatarAlpha);

                combinedAvatar.enabled = avatarAlpha > 0;
                impostorRenderer.enabled = avatarAlpha == 0;
                return;
            }

            this.lodIndex = lodIndex;

            transitionCTS?.Cancel();
            transitionCTS = new CancellationTokenSource();
            Transition(transitionCTS.Token);
        }

        private Renderer CreateImpostor()
        {
            GameObject quadImpostorContainer = Object.Instantiate(Resources.Load<GameObject>("QuadImpostor"), impostorContainer.transform);
            return quadImpostorContainer.GetComponent<Renderer>();
        }

        private async UniTaskVoid Transition(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                throw new OperationCanceledException();

            try
            {
                combinedAvatar.enabled = true;
                impostorRenderer.enabled = true;
                float targetAvatarAlpha = lodIndex <= 1 ? 1f : 0f;
                while (!Mathf.Approximately(targetAvatarAlpha, avatarAlpha))
                {
                    avatarAlpha = Mathf.MoveTowards(avatarAlpha, targetAvatarAlpha, (1f / TRANSITION_DURATION) * Time.deltaTime);
                    UpdateAlpha(avatarAlpha);
                    await UniTask.NextFrame(ct);
                }

                UpdateSSAO(lodIndex);
                UpdateFacialFeatures(lodIndex);

                combinedAvatar.enabled = avatarAlpha > 0;
                impostorRenderer.enabled = avatarAlpha == 0;
            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
            }
        }

        private void UpdateAlpha(float avatarAlpha)
        {
            // Validate that sharedMaterials must be used here
            Material[] mats = combinedAvatar.sharedMaterials;
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].SetFloat(AvatarSystemUtils.DitherFade, avatarAlpha);
            }

            Material impostorMaterial = impostorRenderer.material;
            //TODO implement dither in Unlit shader
            Color current = impostorMaterial.GetColor(AvatarSystemUtils._BaseColor);
            current.a = 1f - avatarAlpha;
            impostorMaterial.SetColor(AvatarSystemUtils._BaseColor, current);
        }

        private void UpdateSSAO(int lodIndex)
        {
            Material[] mats = combinedAvatar.sharedMaterials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (lodIndex == 0)
                    mats[j].DisableKeyword(AvatarSystemUtils.SSAO_OFF_KEYWORD);
                else
                    mats[j].EnableKeyword(AvatarSystemUtils.SSAO_OFF_KEYWORD);
            }
        }

        private void UpdateFacialFeatures(int lodIndex)
        {
            foreach (Renderer facialFeature in facialFeatures)
            {
                facialFeature.enabled = lodIndex == 0;
            }
        }

        public void Dispose()
        {
            transitionCTS?.Cancel();
            transitionCTS = new CancellationTokenSource();
        }

        ~LOD()
        {
            if (impostorRenderer != null)
                Object.Destroy(impostorRenderer.gameObject);
        }
    }

    public class NoLODs : ILOD
    {
        public int lodIndex { get; }

        public void Bind(Renderer combinedAvatar, IEnumerable<Renderer> facialFeatures) { }
        public void SetLodIndex(int lodIndex, bool inmediate = false) { }
        public void SetImpostorTexture(Texture2D texture) { }
        public void SetImpostorTint(Color color) { }

        public void Dispose() { }
    }
}