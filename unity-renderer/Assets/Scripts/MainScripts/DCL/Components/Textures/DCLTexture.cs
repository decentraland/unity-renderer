using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System;
using System.Collections;
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

        protected Model model;
        AssetPromise_Texture texturePromise = null;

        public TextureWrapMode unityWrap;
        public FilterMode unitySamplingMode;
        public Texture2D texture;

        public DCLTexture(DCL.Controllers.ParcelScene scene) : base(scene)
        {
        }

        public static IEnumerator FetchFromComponent(ParcelScene scene, string componentId,
            System.Action<Texture2D> OnFinish)
        {
            yield return FetchTextureComponent(scene, componentId, (dclTexture) => { OnFinish?.Invoke(dclTexture.texture); });
        }

        public static IEnumerator FetchTextureComponent(ParcelScene scene, string componentId,
            System.Action<DCLTexture> OnFinish)
        {
            if (!scene.disposableComponents.ContainsKey(componentId))
            {
                Debug.Log($"couldn't fetch texture, the DCLTexture component with id {componentId} doesn't exist");
                yield break;
            }

            DCLTexture textureComponent = scene.disposableComponents[componentId] as DCLTexture;
            if (textureComponent == null)
            {
                Debug.Log($"couldn't fetch texture, the shared component with id {componentId} is NOT a DCLTexture");
                yield break;
            }

            if (textureComponent.texture == null)
            {
                while (textureComponent.texture == null)
                {
                    yield return null;

                    if (textureComponent.texture != null)
                    {
                        if (OnFinish != null)
                        {
                            OnFinish.Invoke(textureComponent);
                        }

                        yield break;
                    }
                }
            }
            else if (OnFinish != null)
            {
                OnFinish.Invoke(textureComponent);
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            model = SceneController.i.SafeFromJson<Model>(newJson);

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

            if (texture == null && !string.IsNullOrEmpty(model.src))
            {
                bool isBase64 = model.src.Contains("image/png;base64");

                if (isBase64)
                {
                    string base64Data = model.src.Substring(model.src.IndexOf(',') + 1);

                    // The used texture variable can't be null for the ImageConversion.LoadImage to work
                    if (texture == null)
                    {
                        texture = new Texture2D(1, 1);
                    }

                    if (!ImageConversion.LoadImage(texture, Convert.FromBase64String(base64Data)))
                    {
                        Debug.LogError($"DCLTexture with id {id} couldn't parse its base64 image data.");
                    }

                    if (texture != null)
                    {
                        texture.wrapMode = unityWrap;
                        texture.filterMode = unitySamplingMode;
                        texture.Compress(false);
                        texture.Apply(unitySamplingMode != FilterMode.Point, true);
                    }
                }
                else
                {
                    string contentsUrl = string.Empty;
                    bool isExternalURL = model.src.Contains("http://") || model.src.Contains("https://");

                    if (isExternalURL)
                        contentsUrl = model.src;
                    else
                        scene.contentProvider.TryGetContentsUrl(model.src, out contentsUrl);

                    if (!string.IsNullOrEmpty(contentsUrl))
                    {
                        if (texturePromise != null)
                            AssetPromiseKeeper_Texture.i.Forget(texturePromise);

                        texturePromise = new AssetPromise_Texture(contentsUrl, unityWrap, unitySamplingMode, storeDefaultTextureInAdvance: true);
                        texturePromise.OnSuccessEvent += (x) => texture = x.texture;
                        texturePromise.OnFailEvent += (x) => texture = null;

                        AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                        yield return texturePromise;
                    }
                }
            }
        }

        public virtual void AttachTo(PBRMaterial material)
        {
        }

        public virtual void AttachTo(BasicMaterial material)
        {
        }

        public virtual void AttachTo(UIImage image)
        {
        }

        public virtual void DetachFrom(PBRMaterial material)
        {
        }

        public virtual void DetachFrom(BasicMaterial material)
        {
        }

        public virtual void DetachFrom(UIImage image)
        {
        }

        public override void Dispose()
        {
            if (texturePromise != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                texturePromise = null;
                Utils.SafeDestroy(texture);
            }

            base.Dispose();
        }
    }
}