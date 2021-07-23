using UnityEngine;

namespace DCL
{
    // TODO: Cope with avatar shapes recycled from the pool, delete/forget old fetched snapshot
    public class AvatarLODController
    {
        private const bool ONLY_GENERIC_IMPOSTORS = false;
        private const string LOD_TEXTURE_SHADER_VAR = "_BaseMap";

        // 2048x2048 atlas with 8 512x1024 snapshot-sprites
        private const int GENERIC_IMPOSTORS_ATLAS_COLUMNS = 4;
        private const int GENERIC_IMPOSTORS_ATLAS_ROWS = 2;

        public Transform transform;
        public MeshRenderer meshRenderer;
        public Mesh mesh;

        public delegate void LODToggleEventDelegate(bool newValue);
        public event LODToggleEventDelegate OnLODToggle;

        public void SetImpostorTexture(Texture2D impostorTexture)
        {
            if (ONLY_GENERIC_IMPOSTORS || impostorTexture == null)
                return;

            ResetMeshUVs();

            meshRenderer.material.SetTexture(LOD_TEXTURE_SHADER_VAR, impostorTexture);
        }

        public void RandomizeAndApplyGenericImpostor()
        {
            // TODO: change naming and type to Vector2 spriteSize ??
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

            meshRenderer.gameObject.SetActive(enabled);

            OnLODToggle?.Invoke(enabled);
        }
    }
}