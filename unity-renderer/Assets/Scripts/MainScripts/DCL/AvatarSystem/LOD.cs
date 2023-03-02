using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Shaders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvatarSystem
{
    public class LOD : ILOD
    {
        public const float IMPOSTOR_MOVEMENT_INTERPOLATION = 1.79f;
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

        internal Renderer combinedAvatar;
        internal Renderer impostorRenderer;
        internal MeshFilter impostorMeshFilter;
        private float avatarAlpha;

        private CancellationTokenSource transitionCTS;
        private CancellationTokenSource billboardLookAtCameraCTS;
        private string VISIBILITY_CONSTRAIN_IN_IMPOSTOR = "in_impostor";
        private string VISIBILITY_CONSTRAIN_IN_LOD1 = "in_LOD1";
        BaseVariable<Transform> cameraTransform = DataStore.i.camera.transform;

        public LOD(GameObject impostorContainer, IVisibility visibility, IAvatarMovementController avatarMovementController)
        {
            this.impostorContainer = impostorContainer;
            this.visibility = visibility;
            // TODO Once the AvatarMovementController is completly ported into the AvatarSystem we can decouple it from the LOD
            this.avatarMovementController = avatarMovementController;
        }

        internal void EnsureImpostor()
        {
            if (impostorRenderer != null && impostorMeshFilter != null)
                return;
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
            EnsureImpostor();

            if (texture == null)
                AvatarRendererHelpers.RandomizeAndApplyGenericImpostor(impostorMeshFilter.mesh, impostorRenderer.material);
            else
                AvatarRendererHelpers.SetImpostorTexture(texture, impostorMeshFilter.mesh, impostorRenderer.material);
        }

        public void SetImpostorTint(Color color)
        {
            EnsureImpostor();

            AvatarRendererHelpers.SetImpostorTintColor(impostorRenderer.material, color);
        }

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
                UpdateSSAO(combinedAvatar, lodIndex);
                UpdateAlpha(avatarAlpha);
                UpdateMovementLerping(lodIndex);

                if (avatarAlpha > 0)
                    visibility.RemoveCombinedRendererConstrain(VISIBILITY_CONSTRAIN_IN_IMPOSTOR);
                else
                    visibility.AddCombinedRendererConstrain(VISIBILITY_CONSTRAIN_IN_IMPOSTOR);

                if (lodIndex <= 0)
                    visibility.RemoveFacialFeaturesConstrain(VISIBILITY_CONSTRAIN_IN_LOD1);
                else
                    visibility.AddFacialFeaturesConstrain(VISIBILITY_CONSTRAIN_IN_LOD1);

                SetImpostorEnabled(avatarAlpha == 0);
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
            EnsureImpostor();

            try
            {
                visibility.RemoveCombinedRendererConstrain(VISIBILITY_CONSTRAIN_IN_IMPOSTOR);
                impostorRenderer.enabled = true;
                float targetAvatarAlpha = lodIndex <= 1 ? 1f : 0f;
                while (!Mathf.Approximately(targetAvatarAlpha, avatarAlpha))
                {
                    avatarAlpha = Mathf.MoveTowards(avatarAlpha, targetAvatarAlpha, (1f / TRANSITION_DURATION) * Time.deltaTime);
                    UpdateAlpha(avatarAlpha);
                    await UniTask.NextFrame(ct);
                }

                UpdateSSAO(combinedAvatar, lodIndex);
                UpdateMovementLerping(lodIndex);

                if (avatarAlpha > 0)
                    visibility.RemoveCombinedRendererConstrain(VISIBILITY_CONSTRAIN_IN_IMPOSTOR);
                else
                    visibility.AddCombinedRendererConstrain(VISIBILITY_CONSTRAIN_IN_IMPOSTOR);

                if (lodIndex <= 0)
                    visibility.RemoveFacialFeaturesConstrain(VISIBILITY_CONSTRAIN_IN_LOD1);
                else
                    visibility.AddFacialFeaturesConstrain(VISIBILITY_CONSTRAIN_IN_LOD1);

                SetImpostorEnabled(avatarAlpha == 0);
            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
            }
        }

        private void SetImpostorEnabled(bool enabled)
        {
            EnsureImpostor();

            impostorRenderer.enabled = enabled;
            billboardLookAtCameraCTS?.Cancel();
            billboardLookAtCameraCTS?.Dispose();
            if (enabled)
            {
                billboardLookAtCameraCTS = new CancellationTokenSource();
                BillboardLookAtCamera(billboardLookAtCameraCTS.Token);
            }
            else
            {
                billboardLookAtCameraCTS = null;
            }
        }

        internal void UpdateAlpha(float avatarAlpha)
        {
            if (combinedAvatar == null)
                return;

            EnsureImpostor();

            Material[] mats = combinedAvatar.sharedMaterials;
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].SetFloat(ShaderUtils.DitherFade, avatarAlpha);
            }

            Material impostorMaterial = impostorRenderer.material;
            //TODO implement dither in Unlit shader
            Color current = impostorMaterial.GetColor(ShaderUtils.BaseColor);
            current.a = 1f - avatarAlpha;
            impostorMaterial.SetColor(ShaderUtils.BaseColor, current);
        }

        internal static void UpdateSSAO(Renderer renderer, int lodIndex)
        {
            if (renderer == null)
                return;

            Material[] mats = renderer.sharedMaterials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (lodIndex <= 0)
                    mats[j].DisableKeyword(ShaderUtils.SSAO_OFF_KEYWORD);
                else
                    mats[j].EnableKeyword(ShaderUtils.SSAO_OFF_KEYWORD);
            }
        }

        internal void UpdateMovementLerping(int lodIndex) { avatarMovementController.SetMovementLerpWait(lodIndex >= 2 ? IMPOSTOR_MOVEMENT_INTERPOLATION : 0f); }

        private async UniTaskVoid BillboardLookAtCamera(CancellationToken ct)
        {
            while (true)
            {
                SetBillboardRotation(cameraTransform.Get());
                await UniTask.WaitForEndOfFrame(ct).AttachExternalCancellation(ct);
            }
        }

        internal void SetBillboardRotation(Transform lookAt)
        {
            EnsureImpostor();
            impostorRenderer.transform.LookAt(lookAt);
            impostorRenderer.transform.eulerAngles = Vector3.Scale(impostorRenderer.transform.eulerAngles, Vector3.up);
        }

        public void Dispose()
        {
            transitionCTS?.Cancel();
            transitionCTS?.Dispose();
            transitionCTS = null;

            billboardLookAtCameraCTS?.Cancel();
            billboardLookAtCameraCTS?.Dispose();
            billboardLookAtCameraCTS = null;

            if (impostorRenderer != null)
                Object.Destroy(impostorRenderer.gameObject);
        }
    }
}
