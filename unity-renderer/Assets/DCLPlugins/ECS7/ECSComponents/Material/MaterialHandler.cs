using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.Shaders;
using Decentraland.Common;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class MaterialHandler : IECSComponentHandler<PBMaterial>
    {
        private PBMaterial lastModel = null;
        internal AssetPromise_Material promiseMaterial;

        private readonly Queue<AssetPromise_Material> activePromises = new Queue<AssetPromise_Material>();
        private readonly IInternalECSComponent<InternalMaterial> materialInternalComponent;
        private readonly IInternalECSComponent<InternalVideoMaterial> videoMaterialInternalComponent;

        public MaterialHandler(IInternalECSComponent<InternalMaterial> materialInternalComponent, IInternalECSComponent<InternalVideoMaterial> videoMaterialInternalComponent)
        {
            this.materialInternalComponent = materialInternalComponent;
            this.videoMaterialInternalComponent = videoMaterialInternalComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            videoMaterialInternalComponent.RemoveFor(scene, entity);
            materialInternalComponent.RemoveFor(scene, entity, new InternalMaterial(null, true));

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

            TextureUnion texture = model.Pbr != null ? model.Pbr.Texture : model.Unlit != null ? model.Unlit.Texture : null;

            List<InternalVideoMaterial.VideoTextureData> videoTextureDatas = new List<InternalVideoMaterial.VideoTextureData>();

            AssetPromise_Material_Model promiseModel;
            AssetPromise_Material_Model.Texture? albedoTexture = texture != null ? CreateMaterialPromiseTextureModel (
                texture.GetTextureUrl(scene),
                texture.GetWrapMode(),
                texture.GetFilterMode(),
                texture.IsVideoTexture()
            ) : null;

            if (texture != null && texture.IsVideoTexture())
            {
                videoTextureDatas.Add(
                    new InternalVideoMaterial.VideoTextureData(texture.GetVideoTextureId(), ShaderUtils.BaseMap));
            }

            if (model.Pbr != null)
            {
                AssetPromise_Material_Model.Texture? alphaTexture = model.Pbr.AlphaTexture != null ? CreateMaterialPromiseTextureModel (
                    model.Pbr.AlphaTexture.GetTextureUrl(scene),
                    model.Pbr.AlphaTexture.GetWrapMode(),
                    model.Pbr.AlphaTexture.GetFilterMode(),
                    model.Pbr.AlphaTexture.IsVideoTexture()
                ) : null;

                AssetPromise_Material_Model.Texture? emissiveTexture = model.Pbr.EmissiveTexture != null ? CreateMaterialPromiseTextureModel (
                    model.Pbr.EmissiveTexture.GetTextureUrl(scene),
                    model.Pbr.EmissiveTexture.GetWrapMode(),
                    model.Pbr.EmissiveTexture.GetFilterMode()
                ) : null;

                AssetPromise_Material_Model.Texture? bumpTexture = model.Pbr.BumpTexture != null ? CreateMaterialPromiseTextureModel (
                    model.Pbr.BumpTexture.GetTextureUrl(scene),
                    model.Pbr.BumpTexture.GetWrapMode(),
                    model.Pbr.BumpTexture.GetFilterMode()
                ) : null;

                if (model.Pbr.AlphaTexture != null && model.Pbr.AlphaTexture.IsVideoTexture())
                {
                    videoTextureDatas.Add(
                        new InternalVideoMaterial.VideoTextureData(model.Pbr.AlphaTexture.GetVideoTextureId(), ShaderUtils.AlphaTexture));
                }

                if (model.Pbr.EmissiveTexture != null && model.Pbr.EmissiveTexture.IsVideoTexture())
                {
                    videoTextureDatas.Add(
                        new InternalVideoMaterial.VideoTextureData(model.Pbr.EmissiveTexture.GetVideoTextureId(), ShaderUtils.EmissionMap));
                }

                if (model.Pbr.BumpTexture != null && model.Pbr.BumpTexture.IsVideoTexture())
                {
                    videoTextureDatas.Add(
                        new InternalVideoMaterial.VideoTextureData(model.Pbr.BumpTexture.GetVideoTextureId(), ShaderUtils.BumpMap));
                }

                promiseModel = CreatePBRMaterialPromiseModel(model, albedoTexture, alphaTexture, emissiveTexture, bumpTexture);
            }
            else
            {
                promiseModel = CreateBasicMaterialPromiseModel(model, albedoTexture);
            }

            promiseMaterial = new AssetPromise_Material(promiseModel);
            promiseMaterial.OnSuccessEvent += materialAsset =>
            {
                if (videoTextureDatas.Count > 0)
                    videoMaterialInternalComponent.PutFor(scene, entity, new InternalVideoMaterial(materialAsset.material, videoTextureDatas));
                materialInternalComponent.PutFor(scene, entity, new InternalMaterial(materialAsset.material, promiseModel.castShadows));

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
                model.GetAlphaTest(), model.GetCastShadows(), model.GetAlbedoColor().ToUnityColor(), model.GetEmissiveColor().ToUnityColor(),
                model.GetReflectiveColor().ToUnityColor(), (AssetPromise_Material_Model.TransparencyMode)model.GetTransparencyMode(), model.GetMetallic(),
                model.GetRoughness(), model.GetSpecularIntensity(),
                model.GetEmissiveIntensity(), model.GetDirectIntensity());
        }

        private static AssetPromise_Material_Model CreateBasicMaterialPromiseModel(PBMaterial model, AssetPromise_Material_Model.Texture? albedoTexture)
        {
            return AssetPromise_Material_Model.CreateBasicMaterial(albedoTexture, model.GetAlphaTest(), model.GetDiffuseColor().ToUnityColor());
        }

        private static AssetPromise_Material_Model.Texture? CreateMaterialPromiseTextureModel(string textureUrl, UnityEngine.TextureWrapMode wrapMode, FilterMode filterMode, bool videoTexture = false)
        {
            if (string.IsNullOrEmpty(textureUrl))
                return null;

            return new AssetPromise_Material_Model.Texture(textureUrl, wrapMode, filterMode, videoTexture);
        }
    }
}
