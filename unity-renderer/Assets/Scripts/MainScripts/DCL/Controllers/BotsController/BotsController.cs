using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Google.Protobuf;
using UnityEngine;
using DCL.Configuration;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace DCL.Bots
{
    public class BotsController : IBotsController
    {
        private const string ALL_WEARABLES_FETCH_BASE_URL = "https://peer.decentraland.org/content/deployments";
        private const string ALL_WEARABLES_FETCH_FIRST_URL = ALL_WEARABLES_FETCH_BASE_URL + "?entityType=wearable&onlyCurrentlyPointed=true";

        private ParcelScene globalScene;
        private List<string> instantiatedBots = new List<string>();
        private List<string> eyesWearableIds = new List<string>();
        private List<string> eyebrowsWearableIds = new List<string>();
        private List<string> mouthWearableIds = new List<string>();
        private List<string> hairWearableIds = new List<string>();
        private List<string> facialWearableIds = new List<string>();
        private List<string> upperBodyWearableIds = new List<string>();
        private List<string> lowerBodyWearableIds = new List<string>();
        private List<string> feetWearableIds = new List<string>();
        private List<string> bodyshapeWearableIds = new List<string>();

        private IEnumerator EnsureGlobalSceneAndCatalog()
        {
            if (globalScene != null)
                yield break;

            globalScene = Environment.i.world.state.loadedScenes[Environment.i.world.state.globalSceneIds[0]] as ParcelScene;

            CatalogController.wearableCatalog.Clear();

            yield return GetAllWearableItems(ALL_WEARABLES_FETCH_FIRST_URL);
        }

        // TODO: Move this to a new assembly and make ABConverter Client.cs use it from there as well
        private IEnumerator GetAllWearableItems(string url)
        {
            string nextPageParams = null;

            yield return Environment.i.platform.webRequest.Get(
                url: url,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 5000,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! wearables couldn't be fetched! -- {webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    var wearablesApiData = JsonUtility.FromJson<WearablesAPIData>(webRequest.downloadHandler.text);
                    PopulateCatalog(wearablesApiData.GetWearableItemsList());

                    nextPageParams = wearablesApiData.pagination.next;
                });

            if (!string.IsNullOrEmpty(nextPageParams))
            {
                // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
                // batches sequentially
                yield return GetAllWearableItems(ALL_WEARABLES_FETCH_BASE_URL + nextPageParams);
            }
        }

        private void PopulateCatalog(List<WearableItem> newWearables)
        {
            foreach (var wearableItem in newWearables)
            {
                switch (wearableItem.data.category)
                {
                    case WearableLiterals.Categories.EYES:
                        eyesWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.EYEBROWS:
                        eyebrowsWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.MOUTH:
                        mouthWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.FEET:
                        feetWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.HAIR:
                        hairWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.FACIAL:
                        facialWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.LOWER_BODY:
                        lowerBodyWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.UPPER_BODY:
                        upperBodyWearableIds.Add(wearableItem.id);
                        break;
                    case WearableLiterals.Categories.BODY_SHAPE:
                        bodyshapeWearableIds.Add(wearableItem.id);
                        break;
                }
            }

            CatalogController.i.AddWearablesToCatalog(newWearables);
        }

        public IEnumerator InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config)
        {
            yield return EnsureGlobalSceneAndCatalog();

            if (config.xPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
                config.xPos = DCLCharacterController.i.characterPosition.unityPosition.x;

            if (config.yPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
                config.yPos = DCLCharacterController.i.characterPosition.unityPosition.y;

            if (config.zPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
                config.zPos = DCLCharacterController.i.characterPosition.unityPosition.z;

            Vector3 randomizedAreaPosition = new Vector3();
            for (int i = 0; i < config.amount; i++)
            {
                randomizedAreaPosition.Set(Random.Range(config.xPos, config.xPos + config.areaWidth), config.yPos, Random.Range(config.zPos, config.zPos + config.areaDepth));
                InstantiateBot(randomizedAreaPosition);
            }
        }

        public IEnumerator InstantiateBotsAtCoords(CoordsInstantiationConfig config)
        {
            if (config.xCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
                config.xCoord = Mathf.Floor(DCLCharacterController.i.characterPosition.worldPosition.x / ParcelSettings.PARCEL_SIZE);

            if (config.yCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
                config.yCoord = Mathf.Floor(DCLCharacterController.i.characterPosition.worldPosition.z / ParcelSettings.PARCEL_SIZE);

            var worldPosConfig = new WorldPosInstantiationConfig()
            {
                amount = config.amount,
                xPos = config.xCoord * ParcelSettings.PARCEL_SIZE,
                yPos = DCLCharacterController.i.characterPosition.unityPosition.y - DCLCharacterController.i.characterController.height / 2,
                zPos = config.yCoord * ParcelSettings.PARCEL_SIZE,
                areaWidth = config.areaWidth,
                areaDepth = config.areaDepth
            };

            yield return InstantiateBotsAtWorldPos(worldPosConfig);
        }

        void InstantiateBot(Vector3 position)
        {
            string entityId = "BOT-" + instantiatedBots.Count;

            AvatarModel avatarModel = new AvatarModel()
            {
                name = entityId,
                hairColor = Color.white,
                eyeColor = Color.white,
                skinColor = Color.white,
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = GetRandomizedWearablesSet()
            };

            globalScene.CreateEntity(entityId);
            globalScene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.AVATAR_SHAPE, avatarModel);
            UpdateEntityTransform(globalScene, entityId, position, Quaternion.identity, Vector3.one);

            instantiatedBots.Add(entityId);
        }

        public void RemoveBot(string targetEntityId)
        {
            if (!instantiatedBots.Contains(targetEntityId))
                return;

            globalScene.RemoveEntity(targetEntityId);
            instantiatedBots.Remove(targetEntityId);
        }

        public void ClearBots()
        {
            while (instantiatedBots.Count > 0)
            {
                RemoveBot(instantiatedBots[0]);
            }
        }

        List<string> GetRandomizedWearablesSet()
        {
            var wearablesSet = new List<string>();

            if (eyesWearableIds.Count > 0)
                wearablesSet.Add(eyesWearableIds[Random.Range(0, eyesWearableIds.Count)]);

            if (eyebrowsWearableIds.Count > 0)
                wearablesSet.Add(eyebrowsWearableIds[Random.Range(0, eyebrowsWearableIds.Count)]);

            if (mouthWearableIds.Count > 0)
                wearablesSet.Add(mouthWearableIds[Random.Range(0, mouthWearableIds.Count)]);

            if (hairWearableIds.Count > 0)
                wearablesSet.Add(hairWearableIds[Random.Range(0, hairWearableIds.Count)]);

            if (facialWearableIds.Count > 0)
                wearablesSet.Add(facialWearableIds[Random.Range(0, facialWearableIds.Count)]);

            if (upperBodyWearableIds.Count > 0)
                wearablesSet.Add(upperBodyWearableIds[Random.Range(0, upperBodyWearableIds.Count)]);

            if (lowerBodyWearableIds.Count > 0)
                wearablesSet.Add(lowerBodyWearableIds[Random.Range(0, lowerBodyWearableIds.Count)]);

            if (feetWearableIds.Count > 0)
                wearablesSet.Add(feetWearableIds[Random.Range(0, feetWearableIds.Count)]);

            if (bodyshapeWearableIds.Count > 0)
                wearablesSet.Add(bodyshapeWearableIds[Random.Range(0, bodyshapeWearableIds.Count)]);

            return wearablesSet;
        }

        void UpdateEntityTransform(ParcelScene scene, string entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PB_Transform pB_Transform = GetPBTransform(position, rotation, scale);
            scene.EntityComponentCreateOrUpdate(
                entityId,
                CLASS_ID_COMPONENT.TRANSFORM,
                System.Convert.ToBase64String(pB_Transform.ToByteArray())
            );
        }

        public PB_Transform GetPBTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PB_Transform pbTranf = new PB_Transform();
            pbTranf.Position = new PB_Vector3();
            pbTranf.Position.X = position.x;
            pbTranf.Position.Y = position.y;
            pbTranf.Position.Z = position.z;
            pbTranf.Rotation = new PB_Quaternion();
            pbTranf.Rotation.X = rotation.x;
            pbTranf.Rotation.Y = rotation.y;
            pbTranf.Rotation.Z = rotation.z;
            pbTranf.Rotation.W = rotation.w;
            pbTranf.Scale = new PB_Vector3();
            pbTranf.Scale.X = scale.x;
            pbTranf.Scale.Y = scale.y;
            pbTranf.Scale.Z = scale.z;
            return pbTranf;
        }
    }
}