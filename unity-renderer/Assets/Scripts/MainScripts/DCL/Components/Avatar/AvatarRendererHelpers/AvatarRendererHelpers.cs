using UnityEngine;

namespace DCL
{
    public static class AvatarRendererHelpers
    {
        private static readonly byte[] questionMarkPNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 8, 0, 0, 0, 8, 8, 2, 0, 0, 0, 75, 109, 41, 220, 0, 0, 0, 65, 73, 68, 65, 84, 8, 29, 85, 142, 81, 10, 0, 48, 8, 66, 107, 236, 254, 87, 110, 106, 35, 172, 143, 74, 243, 65, 89, 85, 129, 202, 100, 239, 146, 115, 184, 183, 11, 109, 33, 29, 126, 114, 141, 75, 213, 65, 44, 131, 70, 24, 97, 46, 50, 34, 72, 25, 39, 181, 9, 251, 205, 14, 10, 78, 123, 43, 35, 17, 17, 228, 109, 164, 219, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130, };
        private static readonly int IMPOSTOR_TEXTURE_PROPERTY = Shader.PropertyToID("_BaseMap");
        private static readonly int IMPOSTOR_TEXTURE_COLOR_PROPERTY = Shader.PropertyToID("_BaseColor");

        // Manually tweaked values
        public static readonly float IMPOSTOR_MOVEMENT_INTERPOLATION = 1.79f;
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
            if (IsQuestionMarkPNG(impostorTexture))
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

        public static void ResetImpostor(Mesh impostorMesh, Material impostorMaterial)
        {
            SetImpostorTexture(transparentPixelTexture, impostorMesh, impostorMaterial);
        }
        
        internal static bool IsQuestionMarkPNG(Texture tex)
        {
            if (!tex)
                return true;

            if (tex.width != 8 || tex.height != 8)
                return false;

            byte[] png1 = (tex as Texture2D).EncodeToPNG();
            for (int i = 0; i < questionMarkPNG.Length; i++)
                if (!png1[i].Equals(questionMarkPNG[i]))
                    return false;
            return true;
        }
    }
}