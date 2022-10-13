using System.Collections;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.ECSComponents
{
    public class MaterialHandler : IECSComponentHandler<PBMaterial>
    {
        [System.Serializable]
        public class ProfileRequestData
        {
            [System.Serializable]
            public class Avatars
            {
                public Avatar avatar;
            }

            [System.Serializable]
            public class Avatar
            {
                public Snapshots snapshots;
            }

            [System.Serializable]
            public class Snapshots
            {
                public string face256;
                public string body;
            }

            public Avatars[] avatars;
        }
        
        private PBMaterial lastModel = null;
        internal AssetPromise_Material promiseMaterial;

        private readonly IInternalECSComponent<InternalMaterial> materialInternalComponent;

        public MaterialHandler(IInternalECSComponent<InternalMaterial> materialInternalComponent)
        {
            this.materialInternalComponent = materialInternalComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            materialInternalComponent.RemoveFor(scene, entity, new InternalMaterial() { material = null });
            AssetPromiseKeeper_Material.i.Forget(promiseMaterial);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBMaterial model)
        {
            if (lastModel != null && lastModel.Equals(model))
                return;

            lastModel = model;

            if (model.AlbedoTextureCase == PBMaterial.AlbedoTextureOneofCase.AvatarTexture)
            {
                CoroutineStarter.Start(FetchUserProfileAvatarSnapshotSrc(model.AvatarTexture.UserId, (albedoTextureUrl) =>
                {
                    if (string.IsNullOrEmpty(albedoTextureUrl))
                        return;
                    
                    AssetPromise_Material_Model.Texture materialModel = new AssetPromise_Material_Model.Texture(albedoTextureUrl,
                        (UnityEngine.TextureWrapMode)model.AvatarTexture.GetWrapMode(),
                        (UnityEngine.FilterMode)model.AvatarTexture.GetFilterMode());
                    
                    CreateAndConfigureMaterialPromise(scene, entity, model, materialModel);
                }));
                
                return;
            }

            CreateAndConfigureMaterialPromise(scene, entity, model, CreateMaterialPromiseTextureModel(model.Texture, scene));
        }

        private IEnumerator FetchUserProfileAvatarSnapshotSrc(string userId, Action<string> finishCallback)
        {
            if (string.IsNullOrEmpty(userId))
                yield break;
         
            string sourceUrl = Environment.i.platform.serviceProviders.catalyst.lambdasUrl + "/profiles?id=" + userId;
            // The sourceUrl request should return an array, with an object
            //      the object has `timerstamp` and `avatars`, `avatars` is an array
            //      we only request a single avatar so with length=1
            //      avatars[0] has the avatar and we have to access to
            //      avatars[0].avatar.snapshots, and the links are
            //      face,face128,face256 and body
            
            // TODO: check if this user data already exists to avoid this fetch.
            yield return GetAvatarUrls(sourceUrl, finishCallback);
        }
        
        private static IEnumerator GetAvatarUrls(string url, Action<string> onURLSuccess)
        {
            yield return Environment.i.platform.webRequest.Get(
                url: url,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 10,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! profile data couldn't be fetched! -- {webRequest.webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    ProfileRequestData[] data = DCL.Helpers.Utils.ParseJsonArray<ProfileRequestData[]>(webRequest.webRequest.downloadHandler.text);
                    string face256Url = data[0]?.avatars[0]?.avatar.snapshots.face256;
                    onURLSuccess?.Invoke(face256Url);
                });
        }

        private void CreateAndConfigureMaterialPromise(IParcelScene scene, IDCLEntity entity, PBMaterial model, AssetPromise_Material_Model.Texture? albedoTexture)
        {
            AssetPromise_Material_Model? promiseModel = null;
            
            if (IsPbrMaterial(model))
            {
                AssetPromise_Material_Model.Texture? alphaTexture = CreateMaterialPromiseTextureModel(model.AlphaTexture, scene);
                AssetPromise_Material_Model.Texture? emissiveTexture = CreateMaterialPromiseTextureModel(model.EmissiveTexture, scene);
                AssetPromise_Material_Model.Texture? bumpTexture = CreateMaterialPromiseTextureModel(model.BumpTexture, scene);
                promiseModel = CreatePBRMaterialPromiseModel(model, albedoTexture, alphaTexture, emissiveTexture, bumpTexture);
            }
            else if (albedoTexture.HasValue)
            {
                promiseModel = CreateBasicMaterialPromiseModel(model, albedoTexture.Value);
            }

            AssetPromise_Material prevPromise = promiseMaterial;

            if (promiseModel.HasValue)
            {
                promiseMaterial = new AssetPromise_Material(promiseModel.Value);
                promiseMaterial.OnSuccessEvent += materialAsset =>
                {
                    materialInternalComponent.PutFor(scene, entity, new InternalMaterial()
                    {
                        material = materialAsset.material
                    });
                };
                AssetPromiseKeeper_Material.i.Keep(promiseMaterial);
            }

            AssetPromiseKeeper_Material.i.Forget(prevPromise);
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

        private static AssetPromise_Material_Model CreateBasicMaterialPromiseModel(PBMaterial model, AssetPromise_Material_Model.Texture albedoTexture)
        {
            return AssetPromise_Material_Model.CreateBasicMaterial(albedoTexture, model.GetAlphaTest());
        }

        private static AssetPromise_Material_Model.Texture? CreateMaterialPromiseTextureModel(PBMaterial.Types.Texture textureModel, IParcelScene scene)
        {
            if (textureModel == null)
                return null;

            if (string.IsNullOrEmpty(textureModel.Src))
                return null;

            if (!scene.contentProvider.TryGetContentsUrl(textureModel.Src, out string url))
                return null;

            return new AssetPromise_Material_Model.Texture(url,
                (UnityEngine.TextureWrapMode)textureModel.GetWrapMode(),
                (UnityEngine.FilterMode)textureModel.GetFilterMode());
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