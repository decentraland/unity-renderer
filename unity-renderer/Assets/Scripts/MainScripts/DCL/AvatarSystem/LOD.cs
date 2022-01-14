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
        private readonly IVisibility visibility;
        private readonly IAvatarMovementController avatarMovementController;

        private Renderer combinedAvatar;
        private readonly Renderer impostorRenderer;
        private readonly MeshFilter impostorMeshFilter;
        private float avatarAlpha;

        private CancellationTokenSource transitionCTS;

        public LOD(GameObject impostorContainer, IVisibility visibility, IAvatarMovementController avatarMovementController)
        {
            this.impostorContainer = impostorContainer;
            this.visibility = visibility;
            // TODO Once the AvatarMovementController is completly ported into the AvatarSystem we can decouple it from the LOD
            this.avatarMovementController = avatarMovementController;
            impostorRenderer = CreateImpostor();

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

        public void Bind(Renderer combinedAvatar)
        {
            this.combinedAvatar = combinedAvatar;
            SetLodIndex(lodIndex, true);
        }

        public void SetLodIndex(int lodIndex, bool inmediate = false)
        {
            if (inmediate)
            {
                avatarAlpha = lodIndex <= 1 ? 1f : 0f;
                UpdateSSAO(lodIndex);
                UpdateAlpha(avatarAlpha);
                UpdateMovementLerping(lodIndex);
                visibility.SetCombinedRendererVisibility(lodIndex <= 1);
                visibility.SetFacialFeaturesVisibility(lodIndex <= 0);
                impostorRenderer.enabled = avatarAlpha == 0;
                return;
            }

            if (lodIndex == this.lodIndex)
                return;

            this.lodIndex = lodIndex;

            transitionCTS?.Cancel();
            transitionCTS?.Dispose();
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
            ct.ThrowIfCancellationRequested();

            try
            {
                visibility.SetCombinedRendererVisibility(true);
                impostorRenderer.enabled = true;
                float targetAvatarAlpha = lodIndex <= 1 ? 1f : 0f;
                while (!Mathf.Approximately(targetAvatarAlpha, avatarAlpha))
                {
                    avatarAlpha = Mathf.MoveTowards(avatarAlpha, targetAvatarAlpha, (1f / TRANSITION_DURATION) * Time.deltaTime);
                    UpdateAlpha(avatarAlpha);
                    await UniTask.NextFrame(ct);
                }

                UpdateSSAO(lodIndex);
                UpdateMovementLerping(lodIndex);

                visibility.SetCombinedRendererVisibility(avatarAlpha > 0);
                visibility.SetFacialFeaturesVisibility(lodIndex == 0);

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
            if (combinedAvatar == null)
                return;

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
            if (combinedAvatar == null)
                return;

            Material[] mats = combinedAvatar.sharedMaterials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (lodIndex == 0)
                    mats[j].DisableKeyword(AvatarSystemUtils.SSAO_OFF_KEYWORD);
                else
                    mats[j].EnableKeyword(AvatarSystemUtils.SSAO_OFF_KEYWORD);
            }
        }

        private void UpdateMovementLerping(int lodIndex) { avatarMovementController.SetMovementLerpWait(lodIndex >= 2 ? AvatarRendererHelpers.IMPOSTOR_MOVEMENT_INTERPOLATION : 0f); }

        public void Dispose()
        {
            transitionCTS?.Cancel();
            transitionCTS?.Dispose();
            transitionCTS = null;
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

        public void Bind(Renderer combinedAvatar) { }
        public void SetLodIndex(int lodIndex, bool inmediate = false) { }
        public void SetImpostorTexture(Texture2D texture) { }
        public void SetImpostorTint(Color color) { }

        public void Dispose() { }
    }
}