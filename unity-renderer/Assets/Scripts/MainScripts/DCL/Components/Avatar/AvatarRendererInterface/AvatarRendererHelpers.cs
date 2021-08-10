using UnityEngine;

namespace DCL
{
    public static class AvatarRendererHelpers
    {
        private static readonly int IMPOSTOR_TEXTURE_PROPERTY = Shader.PropertyToID("_BaseMap");
        private const bool ONLY_GENERIC_IMPOSTORS = false;

        // 2048x2048 atlas with 8 512x1024 snapshot-sprites
        private const int GENERIC_IMPOSTORS_ATLAS_COLUMNS = 4;
        private const int GENERIC_IMPOSTORS_ATLAS_ROWS = 2;

        public static void RandomizeAndApplyGenericImpostor(Mesh impostorMesh)
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

        private static void ResetImpostorMeshUVs(Mesh impostorMesh)
        {
            Vector2[] uvs = new Vector2[4]; // Quads have only 4 vertices
            uvs[0].Set(0, 0);
            uvs[1].Set(1, 0);
            uvs[2].Set(0, 1);
            uvs[3].Set(1, 1);
            impostorMesh.uv = uvs;
        }

        public static void SetImpostorTexture(Texture2D impostorTexture, Mesh impostorMesh, Material impostorMaterial)
        {
            if (ONLY_GENERIC_IMPOSTORS || impostorTexture == null)
                return;

            ResetImpostorMeshUVs(impostorMesh);

            impostorMaterial.SetTexture(IMPOSTOR_TEXTURE_PROPERTY, impostorTexture);
        }
    }
}