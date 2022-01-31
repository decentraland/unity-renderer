using UnityEngine;

namespace DCL
{
    public static class AvatarRendererHelpers
    {
        private static readonly int IMPOSTOR_TEXTURE_PROPERTY = Shader.PropertyToID("_BaseMap");
        private static readonly int IMPOSTOR_TEXTURE_COLOR_PROPERTY = Shader.PropertyToID("_BaseColor");

        // Manually tweaked values
        private const float IMPOSTOR_TINT_MIN_DISTANCE = 30f;
        private const float IMPOSTOR_TINT_MAX_DISTANCE = 81.33f;
        private const float IMPOSTOR_TINT_NEAREST_BLACKNESS = 0f;
        private const float IMPOSTOR_TINT_FAREST_BLACKNESS = 0.74f;

        // 2048x2048 atlas with 8 512x1024 snapshot-sprites
        private const int GENERIC_IMPOSTORS_ATLAS_COLUMNS = 4;
        private const int GENERIC_IMPOSTORS_ATLAS_ROWS = 2;

        private static Texture2D genericImpostorsTexture = Resources.Load<Texture2D>("Textures/avatar-impostors-atlas");
        private static Texture2D transparentPixelTexture = Resources.Load<Texture2D>("Textures/transparent-pixel");

        public static void RandomizeAndApplyGenericImpostor(Mesh impostorMesh, Material impostorMaterial)
        {
            SetImpostorTexture(genericImpostorsTexture, impostorMesh, impostorMaterial);

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
            if (TextureUtils.IsQuestionMarkPNG(impostorTexture))
                return;

            ResetImpostorMeshUVs(impostorMesh);

            impostorMaterial.SetTexture(IMPOSTOR_TEXTURE_PROPERTY, impostorTexture);
        }

        /// <summary>
        /// Sets impostor texture tint color ignoring the alpha channel
        /// </summary>
        public static void SetImpostorTintColor(Material impostorMaterial, Color newColor)
        {
            newColor.a = impostorMaterial.GetColor(IMPOSTOR_TEXTURE_COLOR_PROPERTY).a;
            impostorMaterial.SetColor(IMPOSTOR_TEXTURE_COLOR_PROPERTY, newColor);
        }

        public static Color CalculateImpostorTint(float distanceToMainPlayer)
        {
            float initialStep = Mathf.Max(IMPOSTOR_TINT_MIN_DISTANCE, distanceToMainPlayer);
            float tintStep = Mathf.InverseLerp(IMPOSTOR_TINT_MIN_DISTANCE, IMPOSTOR_TINT_MAX_DISTANCE, initialStep);
            float tintValue = Mathf.Lerp(IMPOSTOR_TINT_NEAREST_BLACKNESS, IMPOSTOR_TINT_FAREST_BLACKNESS, tintStep);
            Color newColor = Color.Lerp(Color.white, Color.black, tintValue);

            return newColor;
        }

        public static void ResetImpostor(Mesh impostorMesh, Material impostorMaterial) { SetImpostorTexture(transparentPixelTexture, impostorMesh, impostorMaterial); }
    }
}