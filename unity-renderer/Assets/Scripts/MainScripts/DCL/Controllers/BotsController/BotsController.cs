using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using System.Linq;
using DCL.Controllers;
using DCL.Models;
using DCL.Helpers;
using UnityEngine;
using DCL.Configuration;
using DCLServices.WearablesCatalogService;
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
        private IParcelScene globalScene;
        private List<string> selectedCollections = new List<string>();
        private List<long> instantiatedBots = new List<long>();
        private List<string> eyesWearableIds = new List<string>();
        private List<string> eyebrowsWearableIds = new List<string>();
        private List<string> mouthWearableIds = new List<string>();
        private List<string> hairWearableIds = new List<string>();
        private List<string> facialWearableIds = new List<string>();
        private List<string> upperBodyWearableIds = new List<string>();
        private List<string> lowerBodyWearableIds = new List<string>();
        private List<string> feetWearableIds = new List<string>();
        private List<string> bodyshapeWearableIds = new List<string>();

        private Coroutine movementRoutine = null;
        private IWearablesCatalogService wearablesCatalogService;

        public BotsController(IWearablesCatalogService wearablesCatalogService)
        {
            this.wearablesCatalogService = wearablesCatalogService;
        }

        /// <summary>
        /// Makes sure the Catalogue with all the wearables has already been loaded, otherwise it loads it
        /// </summary>
        private IEnumerator EnsureGlobalSceneAndCatalog(bool randomCollections = false)
        {
            if (globalScene != null)
                yield break;

            globalScene = Environment.i.world.state.GetGlobalScenes().First();

            wearablesCatalogService.WearablesCatalog.Clear();

            // We stopped using random collections by default because the wearables API changes frequently and is very inconsistent...
            if(randomCollections)
                yield return WearablesFetchingHelper.GetRandomCollections(50, selectedCollections);

            // We add the base wearables collection to make sure we have at least 1 of each avatar body-part
            yield return WearablesFetchingHelper.GetBaseCollections(selectedCollections);

            List<WearableItem> wearableItems = new List<WearableItem>();
            yield return WearablesFetchingHelper.GetWearableItems(BuildCollectionsURL(), wearableItems);

            PopulateCatalog(wearableItems);
        }

        string BuildCollectionsURL()
        {
            if (selectedCollections.Count == 0)
                return null;

            string finalUrl = WearablesFetchingHelper.GetWearablesFetchURL();
            finalUrl += "collectionId=" + selectedCollections[0];
            for (int i = 1; i < selectedCollections.Count; i++)
            {
                finalUrl += "&collectionId=" + selectedCollections[i];
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

            wearablesCatalogService.AddWearablesToCatalog(newWearables);
        }

        private Vector3 playerUnityPosition => CommonScriptableObjects.playerUnityPosition.Get();
        private Vector3 playerWorldPosition => DataStore.i.player.playerWorldPosition.Get();
        private WorldPosInstantiationConfig lastConfigUsed;
        /// <summary>
        /// Instantiates bots using the config file param values. It defaults some uninitialized values using the player's position
        /// </summary>
        /// <param name="config">The config file to be used</param>
        public IEnumerator InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config)
        {
            yield return EnsureGlobalSceneAndCatalog();

            PatchWorldPosInstantiationConfig(config);

            Log($"Instantiating {config.amount} randomized avatars inside a {config.areaWidth}x{config.areaDepth} area positioned at ({config.xPos}, {config.yPos}, {config.zPos})...");

            Vector3 randomizedAreaPosition = new Vector3();
            for (int i = 0; i < config.amount; i++)
            {
                randomizedAreaPosition.Set(Random.Range(config.xPos, config.xPos + config.areaWidth), config.yPos, Random.Range(config.zPos, config.zPos + config.areaDepth));
                InstantiateBot(randomizedAreaPosition);
            }

            Log($"Finished instantiating {config.amount} avatars. They may take some time to appear while their wearables are being loaded.");

            lastConfigUsed = config;

            // TODO: Remove this and add to new entrypoint call in DebugController...
            // StartRandomMovement(0.5f);
        }

        private void PatchWorldPosInstantiationConfig(WorldPosInstantiationConfig config)
        {
            // TODO(Brian): Use nullable types here, this may fail.
            if (config.xPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"X Position value wasn't provided... using player's current unity X Position: {playerUnityPosition.x}");
                config.xPos = playerUnityPosition.x;
            }

            if (config.yPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"Y Position value wasn't provided... using player's current unity Y Position: {playerUnityPosition.y}");
                config.yPos = playerUnityPosition.y;
            }

            if (config.zPos == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                Log($"Z Position value wasn't provided... using player's current unity Z Position: {playerUnityPosition.z}");
                config.zPos = playerUnityPosition.z;
            }
        }

        /// <summary>
        /// Instantiates bots using the config file param values. It defaults some uninitialized values using the player's coords
        /// </summary>
        /// <param name="config">The config file to be used</param>
        public IEnumerator InstantiateBotsAtCoords(CoordsInstantiationConfig config)
        {
            PatchCoordsInstantiationConfig(config);

            var worldPosConfig = new WorldPosInstantiationConfig()
            {
                amount = config.amount,
                xPos = config.xCoord * ParcelSettings.PARCEL_SIZE,
                yPos = playerUnityPosition.y - CHARACTER_HEIGHT / 2,
                zPos = config.yCoord * ParcelSettings.PARCEL_SIZE,
                areaWidth = config.areaWidth,
                areaDepth = config.areaDepth
            };

            Log($"Instantiating {config.amount} randomized avatars inside a {config.areaWidth}x{config.areaDepth} area positioned at ({config.xCoord}, {config.yCoord}) coords...");

            yield return InstantiateBotsAtWorldPos(worldPosConfig);
        }

        private void PatchCoordsInstantiationConfig(CoordsInstantiationConfig config)
        {
            // TODO(Brian): Use nullable types here, this may fail.
            if (config.xCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                config.xCoord = Mathf.Floor(playerWorldPosition.x / ParcelSettings.PARCEL_SIZE);
                Log($"X Coordinate value wasn't provided... using player's current scene base X coordinate: {config.xCoord}");
            }

            if (config.yCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                config.yCoord = Mathf.Floor(playerWorldPosition.z / ParcelSettings.PARCEL_SIZE);
                Log($"Y Coordinate value wasn't provided... using player's current scene base Y coordinate: {config.yCoord}");
            }
        }

        /// <summary>
        /// Instantiates an entity with an AvatarShape component, with randomized wearables, at the given position
        /// </summary>
        /// <param name="position">The world position of the randomized bot</param>
        void InstantiateBot(Vector3 position)
        {
            long entityId = instantiatedBots.Count;

            AvatarModel avatarModel = new AvatarModel()
            {
                id = entityId.ToString(),
                name = entityId.ToString(),
                hairColor = Random.ColorHSV(0, 1, 0, 1, 0.25f, 0.9f),
                eyeColor = Random.ColorHSV(0, 1, 0, 1, 0f, 0.2f),
                skinColor = Random.ColorHSV(0, 1, 0.3f, 1, 0.4f, 0.9f),
                bodyShape = Random.Range(0, 2) == 0 ? WearableLiterals.BodyShapes.FEMALE : WearableLiterals.BodyShapes.MALE,
                wearables = GetRandomizedWearablesSet()
            };

            var entity = globalScene.CreateEntity(entityId);
            UpdateEntityTransform(globalScene, entityId, position, Quaternion.identity, Vector3.one);

            globalScene.componentsManagerLegacy.EntityComponentCreateOrUpdate(entityId, CLASS_ID_COMPONENT.AVATAR_SHAPE, avatarModel);

            instantiatedBots.Add(entityId);
        }

        /// <summary>
        ///Removes an instantiated bot. Every bot has its ID as its avatar name.
        /// </summary>
        /// <param name="targetEntityId">The target bot ID. Every bot has its ID as its avatar name.</param>
        public void RemoveBot(long targetEntityId)
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
            Log("Removed all bots.");
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

        /// <summary>
        /// Starts a coroutine that traverses a % of the instantiated population at that moment and updates their waypoint with a frequency of 'waypointsUpdateTime' in seconds
        /// </summary>
        /// <param name="populationNormalizedPercentage">The population % that will start moving, expressed normalized e.g: 50% would be 0.5f</param>
        /// <param name="waypointsUpdateTime">The time wait in seconds for each waypoints update</param>
        public void StartRandomMovement(CoordsRandomMovementConfig config)
        {
            if (instantiatedBots.Count == 0)
            {
                Log($"Can't start randomized movement if there are no bots instantiated. Please first instantiate some bots.");
                return;
            }

            PatchCoordsRandomMovementConfig(config);

            Log($"Starting randomized movement on {(config.populationNormalizedPercentage * 100)}% of the current population. Randomized waypoints will be inside a {config.areaWidth}x{config.areaDepth} area positioned at ({config.xCoord}, {config.yCoord}) coords. The waypoints update time is {config.waypointsUpdateTime} seconds.");

            StopRandomMovement();

            int instantiatedCount = instantiatedBots.Count;
            int botsAmount = Mathf.Min(Mathf.FloorToInt(instantiatedCount * config.populationNormalizedPercentage), instantiatedCount);

            List<int> randomBotIndices = new List<int>();
            for (int i = 0; i < botsAmount; i++)
            {
                int randomIndex = Random.Range(0, instantiatedCount);

                if (botsAmount == instantiatedCount)
                {
                    randomIndex = i;
                }
                else
                {
                    while (randomBotIndices.Contains(randomIndex))
                    {
                        randomIndex = Random.Range(0, instantiatedCount);
                    }
                }

                randomBotIndices.Add(randomIndex);
            }

            movementRoutine = CoroutineStarter.Start(RandomMovementRoutine(randomBotIndices, config));
        }

        private const float WAYPOINTS_UPDATE_DEFAULT_TIME = 5f;
        private const float CHARACTER_HEIGHT = 1.6f;
        private void PatchCoordsRandomMovementConfig(CoordsRandomMovementConfig config)
        {
            config.populationNormalizedPercentage = Mathf.Clamp(config.populationNormalizedPercentage, 0f, 1f);

            // TODO(Brian): Use nullable types here, this may fail.
            if (config.waypointsUpdateTime == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                config.waypointsUpdateTime = WAYPOINTS_UPDATE_DEFAULT_TIME;
                Log($"waypointsUpdateTime value wasn't provided... using default time: {config.waypointsUpdateTime}");
            }

            if (config.xCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                config.xCoord = Mathf.Floor(lastConfigUsed.xPos / ParcelSettings.PARCEL_SIZE);
                Log($"X Coordinate value wasn't provided... using last bots spawning X coordinate: {config.xCoord}");
            }

            if (config.yCoord == EnvironmentSettings.UNINITIALIZED_FLOAT)
            {
                config.yCoord = Mathf.Floor(lastConfigUsed.zPos / ParcelSettings.PARCEL_SIZE);
                Log($"Y Coordinate value wasn't provided... using last bots spawning Y coordinate: {config.yCoord}");
            }

            if (config.areaWidth == 0)
            {
                config.areaWidth = lastConfigUsed.areaWidth;
                Log($"Area width provided is 0... using last bots spawning area config width: {config.areaWidth}");
            }

            if (config.areaDepth == 0)
            {
                config.areaWidth = lastConfigUsed.areaDepth;
                Log($"Area depth provided is 0... using last bots spawning area config depth: {config.areaWidth}");
            }
        }

        public void StopRandomMovement()
        {
            if (movementRoutine == null)
                return;

            CoroutineStarter.Stop(movementRoutine);
            Log("Stopped bots movement.");
        }

        private float lastMovementUpdateTime;
        IEnumerator RandomMovementRoutine(List<int> targetBots, CoordsRandomMovementConfig config)
        {
            lastMovementUpdateTime = Time.timeSinceLevelLoad;
            while (true)
            {
                float currentTime = Time.timeSinceLevelLoad;
                if (currentTime - lastMovementUpdateTime >= config.waypointsUpdateTime)
                {
                    lastMovementUpdateTime = currentTime;
                    foreach (int targetBotIndex in targetBots)
                    {
                        // Thanks to the avatars movement interpolation, we can just update their entity position to the target position.

                        Vector3 position = new Vector3(config.xCoord * ParcelSettings.PARCEL_SIZE, lastConfigUsed.yPos, config.yCoord * ParcelSettings.PARCEL_SIZE);
                        Vector3 randomizedAreaPosition = new Vector3(Random.Range(position.x, position.x + config.areaWidth),
                            position.y, Random.Range(position.z, position.z + config.areaDepth));


                        UpdateEntityTransform(globalScene, instantiatedBots[targetBotIndex], randomizedAreaPosition, Quaternion.identity, Vector3.one);
                    }
                }

                yield return null;
            }
        }

        void UpdateEntityTransform(IParcelScene scene, long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(
                entityId,
                CLASS_ID_COMPONENT.TRANSFORM,
                DCLTransformUtils.EncodeTransform(position, rotation, scale)
            );
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
