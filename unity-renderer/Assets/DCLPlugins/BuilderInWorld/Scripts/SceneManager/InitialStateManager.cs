using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Cysharp.Threading.Tasks;
using DCL.Builder.Manifest;
using DCL.Configuration;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL.Builder
{
    public interface IInitialStateManager
    {
        /// <summary>
        /// This call will look for the correct manifest of the land. The lookup will be the next
        /// 1. If the land has a published scene with a project ID, we load this project, if not we continue looking
        /// 2. If the scene has been published with the stateless manifest, we create a project from it, if not we continue looking
        /// 3. If the land has a linked project ( by the coords) we load the project, if not we continue looking
        /// 4. If there is not project associated and we cant create it from a stateless manifest, we just create an empty one
        /// </summary>
        /// <param name="builderAPIController"></param>
        /// <param name="landCoords"></param>
        /// <param name="scene"></param>
        /// <param name="parcelSize"></param>
        /// <returns></returns>
        Promise<InitialStateResponse> GetInitialManifest(IBuilderAPIController builderAPIController, string landCoords, Scene scene, Vector2Int parcelSize);
    }

    public class InitialStateManager : IInitialStateManager
    {
        private InitialStateResponse response = new InitialStateResponse();
        private Promise<InitialStateResponse> masterManifestPromise;

        public Promise<InitialStateResponse> GetInitialManifest(IBuilderAPIController builderAPIController, string landCoords, Scene scene, Vector2Int parcelSize)
        {
            masterManifestPromise = new Promise<InitialStateResponse>();
            GetInitialStateManifest(builderAPIController, landCoords, scene, parcelSize);
            return masterManifestPromise;
        }

        /// <summary>
        /// This call will look for the correct manifest of the land. The lookup will be the next
        /// 1. If the land has a published scene with a project ID, we load this project, if not we continue looking
        /// 2. If the scene has been published with the stateless manifest, we create a project from it, if not we continue looking
        /// 3. If the land has a linked project ( by the coords) we load the project, if not we continue looking
        /// 4. If there is not project associated and we cant create it from a stateless manifest, we just create an empty one
        /// </summary>
        /// <param name="builderAPIController"></param>
        /// <param name="landCoords"></param>
        /// <param name="scene"></param>
        /// <param name="parcelSize"></param>
        public async void GetInitialStateManifest(IBuilderAPIController builderAPIController, string landCoords, Scene scene, Vector2Int parcelSize)
        {
            Manifest.Manifest manifest = null;
            response.hasBeenCreated = false;
            
            //We check if there is a project associated with the land, if there is we load the manifest
            if (!string.IsNullOrEmpty(scene.projectId))
            {
                manifest = await GetProjectById(builderAPIController, scene.projectId);

                //If there is no project associated, we continue to looking
                if (manifest != null)
                {
                    response.manifest = manifest;
                    masterManifestPromise.Resolve(response);
                    return;
                }
            }
            
            //We try to look for coordinates manifest
            manifest = await GetManifestByCoordinates(builderAPIController, landCoords);

            //If we can't find a project associated with the land coords, we continue to looking
            if (manifest != null)
            {
                response.manifest = manifest;
                masterManifestPromise.Resolve(response);
                return;
            }
            
            //We look if the scene has been published with a stateful definition
            if (scene.HasContent(BIWSettings.BUILDER_SCENE_STATE_DEFINITION_FILE_NAME))
            {
                manifest = await GetManifestByStatefullDefinition(parcelSize, landCoords, scene);
        
                //If we can't create a manifest from the stateful definition, we continue to looking
                if (manifest != null)
                {
                    response.manifest = manifest;
                    response.hasBeenCreated = true;
                    masterManifestPromise.Resolve(response);
                    return;
                }
            }

            // If there is no builder project deployed in the land, we just create a new one
            manifest = BIWUtils.CreateEmptyDefaultBuilderManifest(parcelSize, landCoords);
            response.manifest = manifest;
            response.hasBeenCreated = true;
            masterManifestPromise.Resolve(response);
        }

        public async UniTask<Manifest.Manifest> GetManifestByCoordinates(IBuilderAPIController builderAPIController, string landCoords)
        {
            Manifest.Manifest manifest = null;

            var manifestPromise = builderAPIController.GetManifestByCoords(landCoords);
            manifestPromise.Then(resultManifest =>
            {
                manifest = resultManifest;
            });
            manifestPromise.Catch( error =>
            {
                //If the project is not found,the user never created a scene in the land
                if (error != BIWSettings.PROJECT_NOT_FOUND)
                    masterManifestPromise.Reject(error);
            });

            await manifestPromise;
            return manifest;
        }

        public async UniTask<Manifest.Manifest> GetManifestByStatefullDefinition(Vector2Int size, string landCoords, Scene scene)
        {
            Manifest.Manifest manifest = null;

            if (!scene.TryGetHashForContent(BIWSettings.BUILDER_SCENE_STATE_DEFINITION_FILE_NAME, out string stateManifestHash))
                return null;

            if (!scene.TryGetHashForContent(BIWSettings.BUILDER_SCENE_ASSET_FILE_NAME, out string assetHash))
                return null;

            var assetPromise = Environment.i.platform.serviceProviders.catalyst.GetContent(assetHash);
            var statelessPromise = Environment.i.platform.serviceProviders.catalyst.GetContent(stateManifestHash);

            (string assetsString, string statelesString) =  await UniTask.WhenAll(assetPromise, statelessPromise);

            try
            {
                AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(assetsString);
            }
            catch { }

            try
            {
                StatelessManifest statelessManifest = JsonConvert.DeserializeObject<StatelessManifest>(statelesString);
                manifest = BIWUtils.CreateEmptyDefaultBuilderManifest(size, landCoords);
                manifest.scene = ManifestTranslator.StatelessToWebBuilderScene(statelessManifest, size);
                manifest.project.scene_id = manifest.scene.id;
            }
            catch { }

            return manifest;
        }

        public async UniTask<Manifest.Manifest> GetProjectById(IBuilderAPIController builderAPIController, string projectId)
        {
            Manifest.Manifest manifest = null;

            var manifestPromise = builderAPIController.GetManifestById(projectId);
            manifestPromise.Then(resultManifest =>
            {
                manifest = resultManifest;
            });
            manifestPromise.Catch( error =>
            {
                //If the project is not found,the user has deleted the project, we move to the next step
                if (error != BIWSettings.PROJECT_NOT_FOUND)
                    masterManifestPromise.Reject(error);
            });

            await manifestPromise;
            return manifest;
        }
    }

    public class InitialStateResponse
    {
        public Manifest.IManifest manifest;
        public bool hasBeenCreated = false;
    }
}