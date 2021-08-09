using System.Collections.Generic;
using UnityEngine;
using DCL.Configuration;

namespace DCL
{
    public class AvatarLODController
    {
        private readonly int LOD_TEXTURE_SHADER_VAR = Shader.PropertyToID("_BaseMap");

        private static Camera snapshotCamera;
        private static readonly RenderTexture snapshotRenderTexture = new RenderTexture(128, 256, 8);

        private Transform avatarTransform;
        private MeshRenderer impostorMeshRenderer;
        private Mesh impostorMesh; // TODO: If we don't animate mesh uvs we can remove this serialized reference
        private List<Renderer> avatarRenderers;
        private Animation avatarAnimation;

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
            }
        }

        public void Initialize(Transform avatarTransform, MeshRenderer impostorMeshRenderer, Mesh impostorMesh, List<Renderer> avatarRenderers, Animation avatarAnimation)
        {
            this.avatarTransform = avatarTransform;
            this.impostorMeshRenderer = impostorMeshRenderer;
            this.impostorMesh = impostorMesh;
            this.avatarRenderers = avatarRenderers;
            this.avatarAnimation = avatarAnimation;

            this.impostorMeshRenderer.material.SetTexture(LOD_TEXTURE_SHADER_VAR, snapshotRenderTexture);
        }

        public MeshRenderer GetImpostorMeshRenderer() { return impostorMeshRenderer; }

        public Transform GetTransform() { return avatarTransform; }

        private void UpdateAvatarMeshes(int layer, bool enabledState)
        {
            int count = avatarRenderers.Count;
            for (var i = 0; i < count; i++)
            {
                avatarRenderers[i].gameObject.layer = layer;
                avatarRenderers[i].enabled = enabledState;
            }

            avatarAnimation.enabled = enabledState;
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

            UpdateSnapshot();

            UpdateAvatarMeshes(PhysicsLayers.characterPreviewLayer, false);

            lastSnapshotsUpdateTime = Time.timeSinceLevelLoad;
        }

        private void UpdateSnapshot()
        {
            // Position snapshot camera next to the target avatar
            snapshotCamera.gameObject.SetActive(true);
            snapshotCamera.transform.SetParent(Camera.main.transform); // TODO: Camera.main throws nullref when we are in the Avatar Editor and we need the reference here
            snapshotCamera.transform.localPosition = Vector3.zero;
            snapshotCamera.transform.forward = (impostorMeshRenderer.transform.position - snapshotCamera.transform.position).normalized;
            snapshotCamera.transform.position = impostorMeshRenderer.transform.position - snapshotCamera.transform.forward * 2f;

            snapshotCamera.targetTexture = snapshotRenderTexture; // TODO: If all impostors share the renderTex, we can configure it just once

            snapshotCamera.Render();
            RenderTexture.active = snapshotRenderTexture;

            snapshotCamera.gameObject.SetActive(false);
        }
    }
}