using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Google.Protobuf;
using UnityEngine;
using DCL.Configuration;
using UnityEngine.Networking;

namespace DCL.Bots
{
    public class BotsController : IBotsController
    {
        private const string ALL_WEARABLES_FETCH_URL = "https://peer.decentraland.org/content/deployments?entityType=wearable&onlyCurrentlyPointed=true";

        private ParcelScene globalScene;
        private int botsCount = 0;
        List<string> eyesWearableIds = new List<string>();
        List<string> eyebrowsWearableIds = new List<string>();
        List<string> mouthWearableIds = new List<string>();
        List<string> hairWearableIds = new List<string>();
        List<string> facialWearableIds = new List<string>();
        List<string> upperBodyWearableIds = new List<string>();
        List<string> lowerBodyWearableIds = new List<string>();
        List<string> feetWearableIds = new List<string>();
        List<string> bodyshapeWearableIds = new List<string>();

        // TODO Implement ClearBots()
        // TODO Implement RemoveBot(string targetEntityId)

        /*public BotsController()
        {
            Random.InitState(); // TODO: Add a way to report the used seed and force it somehow for deterministic tests?
        }*/

        private void EnsureGlobalSceneAndCatalog()
        {
            if (globalScene != null)
                return;

            globalScene = GameObject.FindObjectOfType<GlobalScene>(); // TODO: fetch this in a more direct and performant way?

            ConstructFullCatalog();
        }

        private void ConstructFullCatalog()
        {
            CatalogController.wearableCatalog.Clear();

            var wearableItems = GetAllWearableItems(ALL_WEARABLES_FETCH_URL);
            foreach (var wearableItem in wearableItems)
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

            CatalogController.i.AddWearablesToCatalog(wearableItems);
        }

        private List<WearableItem> GetAllWearableItems(string url, int paginationElementOffset = 0)
        {
            UnityWebRequest w = UnityWebRequest.Get(url + $"&offset={paginationElementOffset}");
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                Debug.LogWarning($"Request error! wearables couldn't be fetched! -- {w.error}");
                return null;
            }

            var wearablesApiData = JsonUtility.FromJson<WearablesAPIData>(w.downloadHandler.text);
            var resultList = wearablesApiData.GetWearableItemsList();

            // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
            // batches sequentially
            if (wearablesApiData.pagination.moreData)
            {
                var nextPageResults = GetAllWearableItems(url, paginationElementOffset + wearablesApiData.pagination.limit);
                resultList.AddRange(nextPageResults);
            }

            return resultList;
        }

        public void InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config)
        {
            // Debug.Log("PRAVS - BotsController - InstantiateBotsAtWorldPos -> " + config);

            EnsureGlobalSceneAndCatalog();

            Vector3 randomizedAreaPosition = new Vector3();
            for (int i = 0; i < config.amount; i++)
            {
                randomizedAreaPosition.Set(Random.Range(config.xPos, config.xPos + config.areaWidth), config.yPos, Random.Range(config.zPos, config.zPos + config.areaDepth));
                InstantiateBot(randomizedAreaPosition);
            }
        }

        public void InstantiateBotsAtCoords(CoordsInstantiationConfig config)
        {
            // Debug.Log("PRAVS - BotsController - InstantiateBotsAtCoords -> " + config);

            var worldPosConfig = new WorldPosInstantiationConfig()
            {
                amount = config.amount,
                xPos = config.xCoord * ParcelSettings.PARCEL_SIZE,
                yPos = 0f, // TODO: Read player's Y position
                zPos = config.yCoord * ParcelSettings.PARCEL_SIZE,
                areaWidth = config.areaWidth,
                areaDepth = config.areaDepth
            };

            InstantiateBotsAtWorldPos(worldPosConfig);
        }

        void InstantiateBot(Vector3 position)
        {
            AvatarModel avatarModel = new AvatarModel()
            {
                name = "BotAvatar",
                hairColor = Color.white,
                eyeColor = Color.white,
                skinColor = Color.white,
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = GetRandomizedWearablesSet()
            };

            string entityId = "BOT-" + botsCount;
            globalScene.CreateEntity(entityId);
            globalScene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.AVATAR_SHAPE, avatarModel);
            UpdateEntityTransform(globalScene, entityId, position, Quaternion.identity, Vector3.one);

            botsCount++;
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