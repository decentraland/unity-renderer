using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using DCL.Helpers;
using Google.Protobuf;
using UnityEngine;
using DCL.Configuration;
using Random = UnityEngine.Random;

namespace DCL.Bots
{
    /// <summary>
    /// Bots Tool: BotsController
    ///
    /// Used to spawn bots/avatarShapes for debugging and profiling purposes.
    /// </summary>
    public class BotsController : IBotsController
    {
        private ParcelScene globalScene;
        private List<string> randomizedCollections = new List<string>();
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

        /// <summary>
        /// Makes sure the Catalogue with all the wearables has already been loaded, otherwise it loads it
        /// </summary>
        private IEnumerator EnsureGlobalSceneAndCatalog()
        {
            if (globalScene != null)
                yield break;

            globalScene = Environment.i.world.state.loadedScenes[Environment.i.world.state.globalSceneIds[0]] as ParcelScene;

            CatalogController.wearableCatalog.Clear();

            yield return WearablesFetchingHelper.GetRandomCollections(20, true, randomizedCollections);

            List<WearableItem> wearableItems = new List<WearableItem>();
            yield return WearablesFetchingHelper.GetWearableItems(BuildRandomizedCollectionsURL(), wearableItems);

            PopulateCatalog(wearableItems);
        }

        string BuildRandomizedCollectionsURL()
        {
            if (randomizedCollections.Count == 0)
                return null;

            string finalUrl = WearablesFetchingHelper.WEARABLES_FETCH_URL;

            finalUrl += "collectionId=" + randomizedCollections[0];
            for (int i = 1; i < randomizedCollections.Count; i++)
            {
                finalUrl += "&collectionId=" + randomizedCollections[i];
            }

            return finalUrl;
        }

        /// <summary>
        /// Populates the catalogue and internal avatar-part divided collections for optimized randomization
        /// </summary>
        /// <param name="newWearables">The list of WearableItem objects to be added to the catalog</param>
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

        /// <summary>
        /// Instantiates bots using the config file param values. It defaults some uninitialized values using the player's position
        /// </summary>
        /// <param name="config">The config file to be used</param>
        public IEnumerator InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config)
        {
            yield return EnsureGlobalSceneAndCatalog();

            if (config.xPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"X Position value wasn't provided... using player's current X Position.");
                config.xPos = DCLCharacterController.i.characterPosition.unityPosition.x;
            }

            if (config.yPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"Y Position value wasn't provided... using player's current Y Position.");
                config.yPos = DCLCharacterController.i.characterPosition.unityPosition.y;
            }

            if (config.zPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"Z Position value wasn't provided... using player's current Z Position.");
                config.zPos = DCLCharacterController.i.characterPosition.unityPosition.z;
            }

            Log($"Instantiating {config.amount} randomized avatars inside a {config.areaWidth}x{config.areaDepth} area positioned at ({config.xPos}, {config.yPos}, {config.zPos})...");

            Vector3 randomizedAreaPosition = new Vector3();
            for (int i = 0; i < config.amount; i++)
            {
                randomizedAreaPosition.Set(Random.Range(config.xPos, config.xPos + config.areaWidth), config.yPos, Random.Range(config.zPos, config.zPos + config.areaDepth));
                InstantiateBot(randomizedAreaPosition);
            }

            Log($"Finished instantiating {config.amount} avatars. They may take some time to appear while their wearables are being loaded.");
        }

        /// <summary>
        /// Instantiates bots using the config file param values. It defaults some uninitialized values using the player's coords
        /// </summary>
        /// <param name="config">The config file to be used</param>
        public IEnumerator InstantiateBotsAtCoords(CoordsInstantiationConfig config)
        {
            if (config.xCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"X Coordinate value wasn't provided... using player's current scene base X coordinate.");
                config.xCoord = Mathf.Floor(DCLCharacterController.i.characterPosition.worldPosition.x / ParcelSettings.PARCEL_SIZE);
            }

            if (config.yCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"Y Coordinate value wasn't provided... using player's current scene base Y coordinate.");
                config.yCoord = Mathf.Floor(DCLCharacterController.i.characterPosition.worldPosition.z / ParcelSettings.PARCEL_SIZE);
            }

            var worldPosConfig = new WorldPosInstantiationConfig()
            {
                amount = config.amount,
                xPos = config.xCoord * ParcelSettings.PARCEL_SIZE,
                yPos = DCLCharacterController.i.characterPosition.unityPosition.y - DCLCharacterController.i.characterController.height / 2,
                zPos = config.yCoord * ParcelSettings.PARCEL_SIZE,
                areaWidth = config.areaWidth,
                areaDepth = config.areaDepth
            };

            Log($"Instantiating {config.amount} randomized avatars inside a {config.areaWidth}x{config.areaDepth} area positioned at ({config.xCoord}, {config.yCoord}) coords...");

            yield return InstantiateBotsAtWorldPos(worldPosConfig);
        }

        /// <summary>
        /// Instantiates an entity with an AvatarShape component, with randomized wearables, at the given position
        /// </summary>
        /// <param name="position">The world position of the randomized bot</param>
        void InstantiateBot(Vector3 position)
        {
            string entityId = "BOT-" + instantiatedBots.Count;

            AvatarModel avatarModel = new AvatarModel()
            {
                name = entityId,
                hairColor = Color.white,
                eyeColor = Color.white,
                skinColor = Color.white,
                bodyShape = Random.Range(0, 2) == 0 ? WearableLiterals.BodyShapes.FEMALE : WearableLiterals.BodyShapes.MALE,
                wearables = GetRandomizedWearablesSet()
            };

            globalScene.CreateEntity(entityId);
            globalScene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.AVATAR_SHAPE, avatarModel);
            UpdateEntityTransform(globalScene, entityId, position, Quaternion.identity, Vector3.one);

            instantiatedBots.Add(entityId);
        }

        /// <summary>
        ///Removes an instantiated bot. Every bot has its ID as its avatar name.
        /// </summary>
        /// <param name="targetEntityId">The target bot ID. Every bot has its ID as its avatar name.</param>
        public void RemoveBot(string targetEntityId)
        {
            if (!instantiatedBots.Contains(targetEntityId))
                return;

            globalScene.RemoveEntity(targetEntityId);
            instantiatedBots.Remove(targetEntityId);
        }

        /// <summary>
        /// Removes all instantiated bots.
        /// </summary>
        public void ClearBots()
        {
            while (instantiatedBots.Count > 0)
            {
                RemoveBot(instantiatedBots[0]);
            }
        }

        /// <summary>
        /// Randomizes a whole avatar set of wearables and returns a list with all the wearable IDs
        /// </summary>
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

        /// <summary>
        /// Logs the tool messages in console regardless of the "Debug.unityLogger.logEnabled" value. 
        /// </summary>
        private void Log(string message)
        {
            bool originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            Debug.Log("BotsController - " + message);

            Debug.unityLogger.logEnabled = originalLogEnabled;
        }
    }
}