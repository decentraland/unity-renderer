using System;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_TextureResource : AssetPromise<Asset_TextureResource>
    {
        private TextureModel model;
        private AssetPromise_Texture texturePromise;
        
        public AssetPromise_TextureResource(TextureModel model)
        {
            this.model = model;
        }
        
        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnCancelLoading()
        {
            if(texturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
        }
        
        internal override void OnForget()
        {
            if(texturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            FilterMode unitySamplingMode = model.samplingMode;
            TextureWrapMode unityWrap = TextureWrapMode.Clamp;
            switch (model.wrap)
            {
                case TextureModel.BabylonWrapMode.CLAMP:
                    unityWrap = TextureWrapMode.Clamp;
                    break;
                case TextureModel.BabylonWrapMode.WRAP:
                    unityWrap = TextureWrapMode.Repeat;
                    break;
                case TextureModel.BabylonWrapMode.MIRROR:
                    unityWrap = TextureWrapMode.Mirror;
                    break;
            }
            
            if (!string.IsNullOrEmpty(model.src))
            {
                bool isBase64 = model.src.Contains("image/png;base64");

                if (isBase64)
                {
                    string base64Data = model.src.Substring(model.src.IndexOf(',') + 1);

                    // The used texture variable can't be null for the ImageConversion.LoadImage to work
                    Texture2D texture = new Texture2D(1, 1);

                    if (!ImageConversion.LoadImage(texture, Convert.FromBase64String(base64Data)))
                    {
                        Debug.LogError($"DCLTexture with model {model} couldn't parse its base64 image data.");
                    }

                    if (texture != null)
                    {
                        texture.wrapMode = unityWrap;
                        texture.filterMode = unitySamplingMode;
                        texture.Compress(false);
                        texture.Apply(unitySamplingMode != FilterMode.Point, true);
                        asset.texture2D = texture;
                    }
                }
                else
                {
                    string contentsUrl = model.src;

                    if (!string.IsNullOrEmpty(contentsUrl))
                    {
                        Texture2D result = null;
                        texturePromise = new AssetPromise_Texture(contentsUrl, unityWrap, unitySamplingMode, storeDefaultTextureInAdvance: true);
                        texturePromise.OnSuccessEvent += (texture) =>
                        {
                            asset.texture2D = texture.texture;
                            OnSuccess?.Invoke();
                        };
                        texturePromise.OnFailEvent += (texture, error) =>
                        {
                            result = null;
                            OnFail?.Invoke(error);
                        };

                        AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                    }
                }
            }
        }

        protected override object GetLibraryAssetCheckId() { return model; }
        public override object GetId() { return model; }
    }
}