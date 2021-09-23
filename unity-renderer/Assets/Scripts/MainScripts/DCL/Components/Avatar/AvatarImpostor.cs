using System;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AvatarImpostor : IAvatarImpostor
    {
        public event Action<float> OnImpostorAlphaValueUpdate;

        private Renderer renderer;
        private Material material;
        private Mesh mesh;
        private AssetPromise_Texture bodySnapshotTexturePromise;

        public AvatarImpostor (Renderer renderer, MeshFilter meshFilter)
        {
            this.renderer = renderer;
            this.mesh = UnityEngine.Object.Instantiate(meshFilter.sharedMesh);
            this.material = new Material(renderer.sharedMaterial);

            renderer.sharedMaterial = material;
            meshFilter.sharedMesh = mesh;
        }

        public void PopulateTexture(string userId)
        {
            if (bodySnapshotTexturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(bodySnapshotTexturePromise);

            UserProfile userProfile = null;

            if (!string.IsNullOrEmpty(userId))
                userProfile = UserProfileController.GetProfileByUserId(userId);

            if (userProfile != null)
            {
                bodySnapshotTexturePromise = new AssetPromise_Texture(userProfile.bodySnapshotURL);
                bodySnapshotTexturePromise.OnSuccessEvent += asset => AvatarImpostorUtils.SetImpostorTexture(asset.texture, mesh, material);
                bodySnapshotTexturePromise.OnFailEvent += asset => AvatarImpostorUtils.RandomizeAndApplyGenericImpostor(mesh);
                AssetPromiseKeeper_Texture.i.Keep(bodySnapshotTexturePromise);
            }
            else
            {
                AvatarImpostorUtils.RandomizeAndApplyGenericImpostor(mesh);
            }
        }

        public void Dispose()
        {
            CleanUp();

            UnityEngine.Object.Destroy(mesh);
            UnityEngine.Object.Destroy(material);
        }

        public void CleanUp()
        {
            if (renderer != null)
                SetVisibility(false);

            if (bodySnapshotTexturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(bodySnapshotTexturePromise);
        }

        public void SetFade(float impostorFade)
        {
            //TODO implement dither in Unlit shader
            Color current = material.GetColor(ShaderUtils.BaseColor);
            current.a = impostorFade;
            material.SetColor(ShaderUtils.BaseColor, current);

            OnImpostorAlphaValueUpdate?.Invoke(impostorFade);
        }

        public void SetVisibility(bool impostorVisibility) { renderer.gameObject.SetActive(impostorVisibility); }

        public void SetForward(Vector3 newForward) { renderer.transform.forward = newForward; }

        public void SetColor(Color newColor) { AvatarImpostorUtils.SetImpostorTintColor(material, newColor); }

        public void SetColorByDistance(float distance)
        {
            SetColor(AvatarImpostorUtils.CalculateImpostorTint(distance));
        }
    }
}