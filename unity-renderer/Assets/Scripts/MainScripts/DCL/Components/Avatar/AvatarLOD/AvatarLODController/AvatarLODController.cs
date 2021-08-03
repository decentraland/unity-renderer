using System.Collections.Generic;
using UnityEngine;
using DCL.Configuration;

namespace DCL
{
    public class AvatarLODController
    {
        private static Camera snapshotCamera;
        private static readonly RenderTexture snapshotRenderTexture = new RenderTexture(512, 1024, 32);

        private const bool ONLY_GENERIC_IMPOSTORS = false;
        private const string LOD_TEXTURE_SHADER_VAR = "_BaseMap";

        // 2048x2048 atlas with 8 512x1024 snapshot-sprites
        private const int GENERIC_IMPOSTORS_ATLAS_COLUMNS = 4;
        private const int GENERIC_IMPOSTORS_ATLAS_ROWS = 2;

        private Transform avatarTransform;
        private MeshRenderer impostorMeshRenderer;
        private Mesh impostorMesh;
        private List<Renderer> avatarRenderers;
        private Texture2D snapshotTex = new Texture2D(snapshotRenderTexture.width, snapshotRenderTexture.height, TextureFormat.RGBA32, false);

        public float lastSnapshotsUpdateTime;
        public delegate void LODToggleEventDelegate(bool newValue);
        public event LODToggleEventDelegate OnLODToggle;

        public AvatarLODController()
        {
            if (snapshotCamera == null)
            {
                GameObject cameraGO = new GameObject("AvatarsLODImpostorCamera");
                snapshotCamera = cameraGO.AddComponent<Camera>();
                cameraGO.SetActive(false);
                snapshotCamera.clearFlags = CameraClearFlags.SolidColor;
                snapshotCamera.backgroundColor = new Color(0, 0, 0, 0);
                snapshotCamera.cullingMask = 1 << PhysicsLayers.characterPreviewLayer;
                snapshotCamera.targetTexture = snapshotRenderTexture;
            }
        }

        public void Initialize(Transform avatarTransform, MeshRenderer impostorMeshRenderer, Mesh impostorMesh, List<Renderer> avatarRenderers)
        {
            this.avatarTransform = avatarTransform;
            this.impostorMeshRenderer = impostorMeshRenderer;
            this.impostorMesh = impostorMesh;
            this.avatarRenderers = avatarRenderers;
        }

        public MeshRenderer GetImpostorMeshRenderer() { return impostorMeshRenderer; }

        public Transform GetTransform() { return avatarTransform; }

        public void SetImpostorTexture(Texture2D impostorTexture)
        {
            if (ONLY_GENERIC_IMPOSTORS || impostorTexture == null)
                return;

            ResetMeshUVs();

            impostorMeshRenderer.material.SetTexture(LOD_TEXTURE_SHADER_VAR, impostorTexture);
        }

        public void RandomizeAndApplyGenericImpostor()
        {
            Vector2 spriteSize = new Vector2(1f / GENERIC_IMPOSTORS_ATLAS_COLUMNS, 1f / GENERIC_IMPOSTORS_ATLAS_ROWS);
            float randomUVXPos = Random.Range(0, GENERIC_IMPOSTORS_ATLAS_COLUMNS) * spriteSize.x;
            float randomUVYPos = Random.Range(0, GENERIC_IMPOSTORS_ATLAS_ROWS) * spriteSize.y;

            Vector2[] uvs = new Vector2[4]; // Quads have only 4 vertices
            uvs[0].Set(randomUVXPos, randomUVYPos);
            uvs[1].Set(randomUVXPos + spriteSize.x, randomUVYPos);
            uvs[2].Set(randomUVXPos, randomUVYPos + spriteSize.y);
            uvs[3].Set(randomUVXPos + spriteSize.x, randomUVYPos + spriteSize.y);
            impostorMesh.uv = uvs;
        }

        private void ResetMeshUVs()
        {
            Vector2[] uvs = new Vector2[4]; // Quads have only 4 vertices
            uvs[0].Set(0, 0);
            uvs[1].Set(1, 0);
            uvs[2].Set(0, 1);
            uvs[3].Set(1, 1);
            impostorMesh.uv = uvs;
        }

        private void UpdateAvatarMeshes(int layer, bool enabledState)
        {
            int count = avatarRenderers.Count;
            for (var i = 0; i < count; i++)
            {
                avatarRenderers[i].gameObject.layer = layer;
                avatarRenderers[i].enabled = enabledState;
            }
        }

        public void ToggleLOD(bool enabled)
        {
            if (impostorMeshRenderer.gameObject.activeSelf == enabled)
                return;

            impostorMeshRenderer.gameObject.SetActive(enabled);

            UpdateAvatarMeshes(PhysicsLayers.defaultLayer, !enabled);
        }

        public void UpdateImpostorSnapshot()
        {
            UpdateAvatarMeshes(PhysicsLayers.characterPreviewLayer, true);

            SetImpostorTexture(TakeSnapshot());

            UpdateAvatarMeshes(PhysicsLayers.characterPreviewLayer, false);

            lastSnapshotsUpdateTime = Time.timeSinceLevelLoad;
        }

        private Texture2D TakeSnapshot()
        {
            // Position snapshot camera next to the target avatar
            snapshotCamera.gameObject.SetActive(true);
            snapshotCamera.transform.SetParent(Camera.main.transform);
            snapshotCamera.transform.localPosition = Vector3.zero;
            snapshotCamera.transform.forward = (impostorMeshRenderer.transform.position - snapshotCamera.transform.position).normalized;
            snapshotCamera.transform.position = impostorMeshRenderer.transform.position + -snapshotCamera.transform.forward * 2f;

            snapshotCamera.Render();
            RenderTexture.active = snapshotRenderTexture;
            snapshotTex.ReadPixels(new Rect(0, 0, snapshotRenderTexture.width, snapshotRenderTexture.height), 0, 0);
            snapshotTex.Apply();

            snapshotCamera.gameObject.SetActive(false);

            return snapshotTex;
        }
    }
}