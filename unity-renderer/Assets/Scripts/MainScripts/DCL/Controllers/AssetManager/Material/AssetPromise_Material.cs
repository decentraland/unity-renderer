using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Material : AssetPromise<Asset_Material>
    {
        private readonly AssetPromise_Material_Model model;

        private AssetPromise_Texture emissiveTexturePromise;
        private AssetPromise_Texture alphaTexturetPromise;
        private AssetPromise_Texture albedoTexturePromise;
        private AssetPromise_Texture bumpTexturePormise;

        private Coroutine loadCoroutine;

        public AssetPromise_Material(AssetPromise_Material_Model model)
        {
            this.model = model;
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnCancelLoading()
        {
            AssetPromiseKeeper_Texture.i.Forget(emissiveTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(alphaTexturetPromise);
            AssetPromiseKeeper_Texture.i.Forget(albedoTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(bumpTexturePormise);

            CoroutineStarter.Stop(loadCoroutine);
            loadCoroutine = null;
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            CoroutineStarter.Stop(loadCoroutine);
            loadCoroutine = CoroutineStarter.Start(CreateMaterial(model, OnSuccess, OnFail));
        }

        public override object GetId()
        {
            return model;
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        private IEnumerator CreateMaterial(AssetPromise_Material_Model model, Action OnSuccess, Action<Exception> OnFail)
        {
            Material material = model.isPbrMaterial ? PBRMaterial.Create() : BasicMaterial.Create();
#if UNITY_EDITOR
            material.name = $"{(model.isPbrMaterial ? "PBRMaterial_" : "BasicMaterial_")}{model.GetHashCode()}";
#endif
            if (model.isPbrMaterial)
            {
                PBRMaterial.SetUpColors(material, model.albedoColor, model.emissiveColor, model.reflectivityColor, model.emissiveIntensity);
                PBRMaterial.SetUpProps(material, model.metallic, model.roughness, model.glossiness, model.specularIntensity, model.directIntensity);
                PBRMaterial.SetUpTransparency(material, model.transparencyMode, model.alphaTexture, model.albedoColor, model.alphaTest);
            }
            else
            {
                BasicMaterial.SetUp(material, model.alphaTest);
            }

            albedoTexturePromise = CreateTexturePromise(model.albedoTexture);
            emissiveTexturePromise = CreateTexturePromise(model.emissiveTexture);
            alphaTexturetPromise = CreateTexturePromise(model.alphaTexture);
            bumpTexturePormise = CreateTexturePromise(model.bumpTexture);

            yield return WaitAll(albedoTexturePromise, emissiveTexturePromise, alphaTexturetPromise, bumpTexturePormise);

            if (albedoTexturePromise?.state == AssetPromiseState.FINISHED)
            {
                material.SetTexture(ShaderUtils.BaseMap, albedoTexturePromise.asset.texture);
            }

            if (model.isPbrMaterial)
            {
                if (emissiveTexturePromise?.state == AssetPromiseState.FINISHED)
                {
                    material.SetTexture(ShaderUtils.EmissionMap, emissiveTexturePromise.asset.texture);
                }
                if (alphaTexturetPromise?.state == AssetPromiseState.FINISHED)
                {
                    material.SetTexture(ShaderUtils.AlphaTexture, alphaTexturetPromise.asset.texture);
                }
                if (bumpTexturePormise?.state == AssetPromiseState.FINISHED)
                {
                    material.SetTexture(ShaderUtils.BumpMap, bumpTexturePormise.asset.texture);
                }
            }

            SRPBatchingHelper.OptimizeMaterial(material);

            asset.material = material;
            asset.emissiveTexturePromise = emissiveTexturePromise;
            asset.alphaTexturetPromise = alphaTexturetPromise;
            asset.albedoTexturePromise = albedoTexturePromise;
            asset.bumpTexturePormise = bumpTexturePormise;

            OnSuccess?.Invoke();
        }

        private static AssetPromise_Texture CreateTexturePromise(AssetPromise_Material_Model.Texture? textureData)
        {
            if (textureData == null)
                return null;

            AssetPromise_Material_Model.Texture texture = textureData.Value;
            var promise = new AssetPromise_Texture(texture.src, texture.wrapMode, texture.filterMode);
            AssetPromiseKeeper_Texture.i.Keep(promise);
            return promise;
        }

        private static IEnumerator WaitAll(AssetPromise_Texture p1, AssetPromise_Texture p2, AssetPromise_Texture p3, AssetPromise_Texture p4)
        {
            if (p1 != null)
                yield return p1;

            if (p2 != null)
                yield return p2;

            if (p3 != null)
                yield return p3;

            if (p4 != null)
                yield return p4;
        }
    }
}