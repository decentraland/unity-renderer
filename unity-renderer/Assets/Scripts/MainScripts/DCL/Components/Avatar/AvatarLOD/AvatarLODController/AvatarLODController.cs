using UnityEngine;

namespace DCL
{
    public class AvatarLODController
    {
        private const bool ONLY_GENERIC_IMPOSTORS = false;
        private const string LOD_TEXTURE_SHADER_VAR = "_BaseMap";
        private readonly int LOD_IMPOSTOR_LAYER = LayerMask.NameToLayer("CharacterPreview");

        private static Camera snapshotCamera;

        // 2048x2048 atlas with 8 512x1024 snapshot-sprites
        private const int GENERIC_IMPOSTORS_ATLAS_COLUMNS = 4;
        private const int GENERIC_IMPOSTORS_ATLAS_ROWS = 2;

        public Transform transform;
        public MeshRenderer meshRenderer;
        public Mesh mesh;

        public delegate void LODToggleEventDelegate(bool newValue);
        public event LODToggleEventDelegate OnLODToggle;

        public AvatarLODController()
        {
            if (snapshotCamera == null)
            {
                GameObject cameraGO = new GameObject("AvatarsLODImpostorCamera");
                snapshotCamera = cameraGO.AddComponent<Camera>();
                cameraGO.SetActive(false);
                snapshotCamera.cullingMask = LOD_IMPOSTOR_LAYER;
            }
        }

        public void SetImpostorTexture(Texture2D impostorTexture)
        {
            if (ONLY_GENERIC_IMPOSTORS || impostorTexture == null)
                return;

            ResetMeshUVs();

            // GameObject.Destroy(meshRenderer.material.GetTexture(LOD_TEXTURE_SHADER_VAR));
            meshRenderer.material.SetTexture(LOD_TEXTURE_SHADER_VAR, impostorTexture);
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
            mesh.uv = uvs;
        }

        private void ResetMeshUVs()
        {
            Vector2[] uvs = new Vector2[4]; // Quads have only 4 vertices
            uvs[0].Set(0, 0);
            uvs[1].Set(1, 0);
            uvs[2].Set(0, 1);
            uvs[3].Set(1, 1);
            mesh.uv = uvs;
        }

        public void ToggleLOD(bool enabled)
        {
            if (meshRenderer.gameObject.activeSelf == enabled)
                return;

            if (enabled)
            {
                SetImpostorTexture(TakeSnapshot());
            }

            meshRenderer.gameObject.SetActive(enabled);

            OnLODToggle?.Invoke(enabled);
        }

        private Texture2D TakeSnapshot()
        {
            // Position snapshot camera next to the target avatar
            snapshotCamera.gameObject.SetActive(true);
            snapshotCamera.transform.SetParent(Camera.main.transform);
            snapshotCamera.transform.localPosition = Vector3.zero;
            snapshotCamera.transform.forward = (meshRenderer.transform.position - snapshotCamera.transform.position).normalized;
            snapshotCamera.transform.position = meshRenderer.transform.position + -snapshotCamera.transform.forward * 2f;

            // GameObject.Destroy(meshRenderer.material.GetTexture(LOD_TEXTURE_SHADER_VAR));

            RenderTexture rt = new RenderTexture(512, 1024, 32);
            snapshotCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            snapshotCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenShot.Apply();

            snapshotCamera.gameObject.SetActive(false);

            // Debug.Break();

            return screenShot;
        }
    }
}