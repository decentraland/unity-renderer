using UnityEngine;

namespace UnityGLTF
{
    // Algorithm taken from https://gamedev.stackexchange.com/questions/92285/unity3d-resize-texture-without-corruption
    public class TextureScale
    {
        public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Point;

            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Point;

            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D nTex = new Texture2D(newWidth, newHeight);
            nTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
            nTex.Apply();

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = null;

            return nTex;
        }
    }
}
