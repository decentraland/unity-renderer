using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DCL
{
    public class DCLTexture : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string src;
            public BabylonWrapMode wrap = BabylonWrapMode.CLAMP;
            public FilterMode samplingMode = FilterMode.Bilinear;
            public bool hasAlpha = false;
        }

        public enum BabylonWrapMode
        {
            CLAMP,
            WRAP,
            MIRROR
        }

        Model model;

        public TextureWrapMode unityWrap;
        public FilterMode unitySamplingMode;
        public Texture2D texture;

        public DCLTexture(DCL.Controllers.ParcelScene scene) : base(scene)
        {
        }

        public override string componentName => "texture";

        public static IEnumerator FetchFromComponent(ParcelScene scene, string componentId, System.Action<Texture2D> OnFinish)
        {
            if (!scene.disposableComponents.ContainsKey(componentId))
                yield break;

            DCLTexture textureComponent = scene.disposableComponents[componentId] as DCLTexture;

            if (textureComponent == null)
                yield break;

            if (textureComponent.texture == null)
            {
                while (textureComponent.texture == null)
                {
                    yield return null;

                    if (textureComponent.texture != null)
                    {
                        if (OnFinish != null)
                            OnFinish.Invoke(textureComponent.texture);

                        yield break;
                    }
                }
            }
            else
            {
                if (OnFinish != null)
                    OnFinish.Invoke(textureComponent.texture);
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = JsonUtility.FromJson<Model>(newJson);

            unitySamplingMode = model.samplingMode;

            switch (model.wrap)
            {
                case BabylonWrapMode.CLAMP:
                    unityWrap = TextureWrapMode.Clamp;
                    break;
                case BabylonWrapMode.WRAP:
                    unityWrap = TextureWrapMode.Repeat;
                    break;
                case BabylonWrapMode.MIRROR:
                    unityWrap = TextureWrapMode.Mirror;
                    break;
            }

            if (texture == null && !string.IsNullOrEmpty( model.src ) )
            {
                bool isBase64 = Regex.Match(model.src, "^base64,/i").Success && !model.src.StartsWith("data:");
                string finalUrl = string.Empty;

                if (!isBase64)
                {
                    string contentsUrl = string.Empty;

                    if (scene.sceneData.TryGetContentsUrl(model.src, out contentsUrl))
                    {
                        finalUrl = contentsUrl;
                    }
                }
                else
                {
                    finalUrl = $"data:image/png;${model.src}";
                }

                if (!string.IsNullOrEmpty(finalUrl))
                {
                    yield return Utils.FetchTexture(finalUrl, (tex) =>
                    {
                        texture = (Texture2D)tex;
                    });
                }

                if (texture != null)
                {
                    texture.wrapMode = unityWrap;
                    texture.filterMode = unitySamplingMode;
                    texture.Apply(unitySamplingMode != FilterMode.Point, true);
                }
            }
        }

        public override void Dispose()
        {
            Object.Destroy(texture);
            base.Dispose();
        }
    }
}
