using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class MaterialHandler : IECSComponentHandler<PBMaterial>
    {
        private PBMaterial lastModel = null;
        internal AssetPromise_Material promiseMaterial;

        private readonly Queue<AssetPromise_Material> activePromises = new Queue<AssetPromise_Material>();
        private readonly IInternalECSComponent<InternalMaterial> materialInternalComponent;

        public MaterialHandler(IInternalECSComponent<InternalMaterial> materialInternalComponent)
        {
            this.materialInternalComponent = materialInternalComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            materialInternalComponent.RemoveFor(scene, entity, new InternalMaterial() { material = null });

            while (activePromises.Count > 0)
            {
                AssetPromiseKeeper_Material.i.Forget(activePromises.Dequeue());
            }
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBMaterial model)
        {
            if (lastModel != null && lastModel.Equals(model))
                return;

            lastModel = model;

            AssetPromise_Material_Model promiseModel;
            AssetPromise_Material_Model.Texture? albedoTexture = model.Texture != null
                ? CreateMaterialPromiseTextureModel(
                    model.Texture.GetTextureUrl(scene),
                    model.Texture.GetWrapMode(),
                    model.Texture.GetFilterMode()
                )
                : null;

            if (IsPbrMaterial(model))
            {
                AssetPromise_Material_Model.Texture? alphaTexture = model.AlphaTexture != null
                    ? CreateMaterialPromiseTextureModel(
                        model.AlphaTexture.GetTextureUrl(scene),
                        model.AlphaTexture.GetWrapMode(),
                        model.AlphaTexture.GetFilterMode()
                    )
                    : null;

                AssetPromise_Material_Model.Texture? emissiveTexture = model.EmissiveTexture != null
                    ? CreateMaterialPromiseTextureModel(
                        model.EmissiveTexture.GetTextureUrl(scene),
                        model.EmissiveTexture.GetWrapMode(),
                        model.EmissiveTexture.GetFilterMode()
                    )
                    : null;

                AssetPromise_Material_Model.Texture? bumpTexture = model.BumpTexture != null
                    ? CreateMaterialPromiseTextureModel(
                        model.BumpTexture.GetTextureUrl(scene),
                        model.BumpTexture.GetWrapMode(),
                        model.BumpTexture.GetFilterMode()
                    )
                    : null;

                promiseModel = CreatePBRMaterialPromiseModel(model, albedoTexture, alphaTexture, emissiveTexture, bumpTexture);
            }
            else
            {
                promiseModel = CreateBasicMaterialPromiseModel(model, albedoTexture);
            }

            promiseMaterial = new AssetPromise_Material(promiseModel);
            promiseMaterial.OnSuccessEvent += materialAsset =>
            {
                materialInternalComponent.PutFor(scene, entity, new InternalMaterial()
                {
                    material = materialAsset.material
                });

                // Run task to forget previous material after update to avoid forgetting a
                // material that has not be changed from the renderers yet, since material change
                // is done by a system during update
                UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                    ForgetPreviousPromises(activePromises, materialAsset);
                });
            };
            promiseMaterial.OnFailEvent += (material, exception) =>
            {
                ForgetPreviousPromises(activePromises, material);
            };
            activePromises.Enqueue(promiseMaterial);
            AssetPromiseKeeper_Material.i.Keep(promiseMaterial);
        }

        private static void ForgetPreviousPromises(Queue<AssetPromise_Material> promises, Asset_Material currentAppliedMaterial)
        {
            if (promises.Count <= 1)
                return;

            while (promises.Count > 1 && promises.Peek().asset != currentAppliedMaterial)
            {
                AssetPromiseKeeper_Material.i.Forget(promises.Dequeue());
            }
        }

        private static AssetPromise_Material_Model CreatePBRMaterialPromiseModel(PBMaterial model,
            AssetPromise_Material_Model.Texture? albedoTexture, AssetPromise_Material_Model.Texture? alphaTexture,
            AssetPromise_Material_Model.Texture? emissiveTexture, AssetPromise_Material_Model.Texture? bumpTexture)
        {
            return AssetPromise_Material_Model.CreatePBRMaterial(albedoTexture, alphaTexture, emissiveTexture, bumpTexture,
                model.GetAlphaTest(), model.GetAlbedoColor().ToUnityColor(), model.GetEmissiveColor().ToUnityColor(),
                model.GetReflectiveColor().ToUnityColor(), (AssetPromise_Material_Model.TransparencyMode)model.GetTransparencyMode(), model.GetMetallic(),
                model.GetRoughness(), model.GetGlossiness(), model.GetSpecularIntensity(),
                model.GetEmissiveIntensity(), model.GetDirectIntensity());
        }

        private static AssetPromise_Material_Model CreateBasicMaterialPromiseModel(PBMaterial model, AssetPromise_Material_Model.Texture? albedoTexture)
        {
            return AssetPromise_Material_Model.CreateBasicMaterial(albedoTexture, model.GetAlphaTest());
        }

        private static AssetPromise_Material_Model.Texture? CreateMaterialPromiseTextureModel(string textureUrl, UnityEngine.TextureWrapMode wrapMode, FilterMode filterMode)
        {
            if (string.IsNullOrEmpty(textureUrl))
                return null;

            return new AssetPromise_Material_Model.Texture(textureUrl, wrapMode, filterMode);
        }

        private static bool IsPbrMaterial(PBMaterial model)
        {
            return (model.AlphaTexture != null || model.BumpTexture != null || model.EmissiveTexture != null
                    || model.HasGlossiness || model.HasMetallic || model.HasRoughness || model.HasDirectIntensity
                    || model.HasEmissiveIntensity || model.HasSpecularIntensity
                    || model.AlbedoColor != null || model.EmissiveColor != null || model.ReflectivityColor != null
                    || model.HasTransparencyMode);
        }
    }
}