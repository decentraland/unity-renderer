using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class TextureArrayHelper
    {
        private static readonly int GLOBAL_AVATAR_TEXTURE_ARRAY = Shader.PropertyToID("_GlobalAvatarTextureArray");
        const int MAX_ID_COUNT = 400;

        public Texture[] textures;
        public Queue<int> availableIds = new Queue<int>();
        private Dictionary<Texture, int> textureToId = new Dictionary<Texture, int>();
        private Texture2DArray textureArray;

        public TextureArrayHelper ()
        {
            textureArray = new Texture2DArray(256, 256, MAX_ID_COUNT, TextureFormat.ARGB32, false, false);

            for ( int i = 0 ; i < MAX_ID_COUNT; i++ )
            {
                availableIds.Enqueue(i);
            }

            textures = new Texture[MAX_ID_COUNT];
            UpdateShaderData();
        }

        public int AddTexture(Texture texture)
        {
            if ( texture == null )
            {
                //Debug.Log("Adding null texture to global texture cache!");
                return -1;
            }

            if ( textureToId.ContainsKey(texture))
                return textureToId[texture];

            int newId = availableIds.Dequeue();
            textures[newId] = texture;

            Texture2D newTexture = ConvertTexture(texture as Texture2D);
            textureArray.SetPixelData(newTexture.GetRawTextureData(), 0, newId, 0);
            textureArray.Apply(false);
            //Debug.Log($"Adding {newId} texture to global texture cache!", textureArray);
            textureToId.Add(texture, newId);
            //UpdateShaderData();
            return newId;
        }

        public static Texture2D ConvertTexture(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(256, 256);

            RenderTexture.active = rt;

            source.filterMode = FilterMode.Point;
            rt.filterMode = FilterMode.Point;

            Graphics.Blit(source, rt);
            RenderTexture.ReleaseTemporary(rt);

            Texture2D nTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            nTex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0, false);
            nTex.Apply(false);

            return nTex;
        }


        public void RemoveTexture(Texture texture)
        {
            if ( !textureToId.ContainsKey(texture))
                return;

            availableIds.Enqueue(textureToId[texture]);
            textureToId.Remove(texture);
        }

        public void UpdateShaderData()
        {
            Shader.SetGlobalTexture(GLOBAL_AVATAR_TEXTURE_ARRAY, textureArray);
        }
    }
}