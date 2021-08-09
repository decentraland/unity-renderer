using System.Collections.Generic;
using UnityEngine;
using DCL.Configuration;

namespace DCL
{
    public class AvatarLODController
    {
        private enum SnapshotDirections
        {
            North,
            East,
            South,
            West,
            COUNT
        }

        private readonly int LOD_TEXTURE_SHADER_VAR = Shader.PropertyToID("_BaseMap");
        private static Camera snapshotCamera;

        private readonly RenderTexture[] snapshotRenderTextures = new RenderTexture[(int)SnapshotDirections.COUNT];

        private Transform avatarTransform;
        private MeshRenderer impostorMeshRenderer;
        private Mesh impostorMesh; // TODO: If we don't animate mesh uvs we can remove this serialized reference
        private List<Renderer> avatarRenderers;
        private Animation avatarAnimation;
        private SnapshotDirections currentCharacterRelativeDirection = SnapshotDirections.North;

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

            for (var i = 0; i < snapshotRenderTextures.Length; i++)
            {
                snapshotRenderTextures[i] = new RenderTexture(128, 256, 8);
            }
        }

        public void Initialize(Transform avatarTransform, MeshRenderer impostorMeshRenderer, Mesh impostorMesh, List<Renderer> avatarRenderers, Animation avatarAnimation)
        {
            this.avatarTransform = avatarTransform;
            this.impostorMeshRenderer = impostorMeshRenderer;
            this.impostorMesh = impostorMesh;
            this.avatarRenderers = avatarRenderers;
            this.avatarAnimation = avatarAnimation;

            InitializeSnapshots();
        }

        // TODO: We should trigger this initialization again if a wearable is changed on the avatar
        private void InitializeSnapshots()
        {
            UpdateAvatarMeshes(PhysicsLayers.characterPreviewLayer, true);
            snapshotCamera.gameObject.SetActive(true);

            // Take N snapshot
            snapshotCamera.targetTexture = snapshotRenderTextures[(int)SnapshotDirections.North];
            snapshotCamera.transform.position = impostorMeshRenderer.transform.position + Vector3.forward * 2;
            snapshotCamera.transform.forward = impostorMeshRenderer.transform.position - snapshotCamera.transform.position;
            snapshotCamera.Render();
            RenderTexture.active = snapshotCamera.targetTexture;

            // Take E snapshot
            snapshotCamera.targetTexture = snapshotRenderTextures[(int)SnapshotDirections.East];
            snapshotCamera.transform.position = impostorMeshRenderer.transform.position + Vector3.right * 2;
            snapshotCamera.transform.forward = impostorMeshRenderer.transform.position - snapshotCamera.transform.position;
            snapshotCamera.Render();
            RenderTexture.active = snapshotCamera.targetTexture;

            // Take S snapshot
            snapshotCamera.targetTexture = snapshotRenderTextures[(int)SnapshotDirections.South];
            snapshotCamera.transform.position = impostorMeshRenderer.transform.position + Vector3.back * 2;
            snapshotCamera.transform.forward = impostorMeshRenderer.transform.position - snapshotCamera.transform.position;
            snapshotCamera.Render();
            RenderTexture.active = snapshotCamera.targetTexture;

            // Take W snapshot
            snapshotCamera.targetTexture = snapshotRenderTextures[(int)SnapshotDirections.West];
            snapshotCamera.transform.position = impostorMeshRenderer.transform.position + Vector3.left * 2;
            snapshotCamera.transform.forward = impostorMeshRenderer.transform.position - snapshotCamera.transform.position;
            snapshotCamera.Render();
            RenderTexture.active = snapshotCamera.targetTexture;

            snapshotCamera.gameObject.SetActive(false);
            UpdateAvatarMeshes(PhysicsLayers.defaultLayer, true);
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

        public void UpdateImpostorSnapshot(Vector3 mainCharacterPosition)
        {
            var characterRelativeDirection = GetCharacterRelativeDirection(mainCharacterPosition);

            if (currentCharacterRelativeDirection == characterRelativeDirection)
                return;

            // Debug.Log("PRAVS - Setting snapshot: " + characterRelativeDirection);

            currentCharacterRelativeDirection = characterRelativeDirection;
            impostorMeshRenderer.material.SetTexture(LOD_TEXTURE_SHADER_VAR, snapshotRenderTextures[(int)currentCharacterRelativeDirection]);
        }

        private SnapshotDirections GetCharacterRelativeDirection(Vector3 mainCharacterPosition)
        {
            Vector3 from = Vector3.forward;
            Vector3 to = (mainCharacterPosition - impostorMeshRenderer.transform.position).normalized;
            to.y = 0;

            var angle = Vector3.SignedAngle(from, to, Vector3.up);

            if (angle >= -45 && angle < 45)
                return SnapshotDirections.North;

            if (angle >= 45 && angle < 135)
                return SnapshotDirections.East;

            if (angle >= -135 && angle < 45)
                return SnapshotDirections.West;

            return SnapshotDirections.South;
        }
    }
}