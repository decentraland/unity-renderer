using DCL;
using DCL.Components;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DCL.Configuration;
using static ProtocolV2;
using Environment = DCL.Environment;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Controllers;
using DCL.Helpers;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.Events;

public static partial class BIWUtils
{
    public static IDCLEntity DuplicateEntity(IParcelScene scene, IDCLEntity entity)
    {
        if (!scene.entities.ContainsKey(entity.entityId))
            return null;

        var sceneController = Environment.i.world.sceneController;
        IDCLEntity newEntity =
            scene.CreateEntity(
                sceneController.entityIdHelper.EntityFromLegacyEntityString(System.Guid.NewGuid().ToString()));

        if (entity.children.Count > 0)
        {
            using (var iterator = entity.children.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    IDCLEntity childDuplicate = DuplicateEntity(scene, iterator.Current.Value);
                    childDuplicate.SetParent(newEntity);
                }
            }
        }

        if (entity.parent != null)
            scene.SetEntityParent(newEntity.entityId, entity.parent.entityId);

        DCLTransform.model.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position);
        DCLTransform.model.rotation = entity.gameObject.transform.rotation;
        DCLTransform.model.scale = entity.gameObject.transform.lossyScale;

        var components = scene.componentsManagerLegacy.GetComponentsDictionary(entity);

        if (components != null)
        {
            foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> component in components)
            {
                scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(newEntity.entityId, component.Key, component.Value.GetModel());
            }
        }

        using (var iterator = scene.componentsManagerLegacy.GetSharedComponents(entity))
        {
            while (iterator.MoveNext())
            {
                ISharedComponent sharedComponent = scene.componentsManagerLegacy.SceneSharedComponentCreate(System.Guid.NewGuid().ToString(), iterator.Current.GetClassId());
                sharedComponent.UpdateFromModel(iterator.Current.GetModel());
                scene.componentsManagerLegacy.SceneSharedComponentAttach(newEntity.entityId, sharedComponent.id);
            }
        }
        
        return newEntity;
    }
    
    public static bool IsParcelSceneSquare(Vector2Int[] parcelsPoints)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (Vector2Int vector in parcelsPoints)
        {
            if (vector.x < minX)
                minX = vector.x;
            if (vector.y < minY)
                minY = vector.y;
            if (vector.x > maxX)
                maxX = vector.x;
            if (vector.y > maxY)
                maxY = vector.y;
        }

        if (maxX - minX != maxY - minY)
            return false;

        int lateralLengh = Math.Abs((maxX - minX) + 1);

        if (parcelsPoints.Length != lateralLengh * lateralLengh)
            return false;

        return true;
    }
    
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static LayerMask GetBIWCulling(LayerMask currentCulling)
    {
        currentCulling += BIWSettings.FX_LAYER;
        return currentCulling;
    }
    
    public static string Vector2INTToString(Vector2Int vector) { return vector.x + "," + vector.y; }

    public static List<Vector2Int> GetLandsToPublishProject(LandWithAccess[] lands, IBuilderScene scene)
    {
        List<Vector2Int> availableLandsToPublish = new List<Vector2Int>();
        List<Vector2Int> totalParcels = new List<Vector2Int>();
        foreach (LandWithAccess land in lands)
        {
            totalParcels.AddRange(land.parcels.ToList());
        }

        Vector2Int sceneSize = GetSceneSize(scene.scene.sceneData.parcels);
        foreach (Vector2Int parcel in totalParcels)
        {
            List<Vector2Int> necessaryParcelsToOwn = new List<Vector2Int>();
            for (int x = 0; x < sceneSize.x; x++)
            {
                for (int y = 0; y < sceneSize.y; y++)
                {
                    necessaryParcelsToOwn.Add(new Vector2Int(parcel.x + x, parcel.y + y));
                }
            }

            int amountOfParcelFounds = 0;
            foreach (Vector2Int parcelToCheck in totalParcels)
            {
                if (necessaryParcelsToOwn.Contains(parcelToCheck))
                    amountOfParcelFounds++;
            }

            if (amountOfParcelFounds == necessaryParcelsToOwn.Count)
                availableLandsToPublish.Add(parcel);
        }
        return availableLandsToPublish;
    }

    public static Vector2Int GetRowsAndColumsFromLand(LandWithAccess landWithAccess)
    {
        Vector2Int result = new Vector2Int();
        int baseX = landWithAccess.baseCoords.x;
        int baseY = landWithAccess.baseCoords.y;

        int amountX = 0;
        int amountY = 0;

        foreach (Vector2Int parcel in landWithAccess.parcels)
        {
            if (parcel.x == baseX)
                amountX++;

            if (parcel.y == baseY)
                amountY++;
        }

        result.x = amountX;
        result.y = amountY;
        return result;
    }

    public static ILand CreateILandFromManifest(IManifest manifest, Vector2Int initialCoord)
    {
        ILand land = new ILand();
        land.sceneId = manifest.project.scene_id;
        land.baseUrl = BIWUrlUtils.GetUrlSceneObjectContent();

        land.mappingsResponse = new MappingsResponse();
        land.mappingsResponse.parcel_id = land.sceneId;
        land.mappingsResponse.root_cid = land.sceneId;
        land.mappingsResponse.contents = new List<ContentServerUtils.MappingPair>();

        land.sceneJsonData = new SceneJsonData();
        land.sceneJsonData.main = "bin/game.js";
        land.sceneJsonData.scene = new SceneParcels();
        land.sceneJsonData.scene.@base = initialCoord.x + "," + initialCoord.y;

        int amountOfParcels = manifest.project.rows * manifest.project.cols;
        land.sceneJsonData.scene.parcels = new string[amountOfParcels];

        int baseX = initialCoord.x;
        int baseY = initialCoord.y;

        int currentPositionInRow = 0;
        for (int i = 0; i < amountOfParcels; i++ )
        {
            land.sceneJsonData.scene.parcels[i] = baseX + "," + baseY;
            currentPositionInRow++;
            baseX++;
            if (currentPositionInRow >= manifest.project.rows)
            {
                baseX = initialCoord.x;
                baseY++;
                currentPositionInRow = 0;
            }
        }

        return land;
    }

    public static ILand CreateILandFromParcelScene(IParcelScene scene)
    {
        ILand land = new ILand();
        land.sceneId = scene.sceneData.id;
        land.baseUrl = scene.sceneData.baseUrl;
        land.baseUrlBundles = scene.sceneData.baseUrlBundles;

        land.mappingsResponse = new MappingsResponse();
        land.mappingsResponse.parcel_id = land.sceneId;
        land.mappingsResponse.root_cid = land.sceneId;
        land.mappingsResponse.contents = scene.sceneData.contents;

        land.sceneJsonData = new SceneJsonData();
        land.sceneJsonData.main = "bin/game.js";
        land.sceneJsonData.scene = new SceneParcels();
        land.sceneJsonData.scene.@base = scene.sceneData.basePosition.ToString();
        land.sceneJsonData.scene.parcels = new string[scene.sceneData.parcels.Length];

        int count = 0;
        foreach (Vector2Int parcel in scene.sceneData.parcels)
        {
            land.sceneJsonData.scene.parcels[count] = parcel.x + "," + parcel.y;
            count++;
        }

        return land;
    }

    public static void AddSceneMappings(Dictionary<string, string> contents, string baseUrl, LoadParcelScenesMessage.UnityParcelScene data)
    {
        if (data == null)
            data = new LoadParcelScenesMessage.UnityParcelScene();

        data.baseUrl = baseUrl;
        if (data.contents == null)
            data.contents = new List<ContentServerUtils.MappingPair>();

        foreach (KeyValuePair<string, string> content in contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            bool found = false;
            foreach (ContentServerUtils.MappingPair mappingPairToCheck in data.contents)
            {
                if (mappingPairToCheck.file == mappingPair.file)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                data.contents.Add(mappingPair);
        }
    }

    public static void RemoveAssetsFromCurrentScene()
    {
        //We remove the old assets to they don't collide with the new ones
        foreach (var catalogItem in DataStore.i.builderInWorld.currentSceneCatalogItemDict.GetValues())
        {
            AssetCatalogBridge.i.RemoveSceneObjectToSceneCatalog(catalogItem.id);
        }
        DataStore.i.builderInWorld.currentSceneCatalogItemDict.Clear();
    }

    public static void ShowGenericNotification(string message, DCL.NotificationModel.Type type = DCL.NotificationModel.Type.GENERIC, float timer = BIWSettings.LAND_NOTIFICATIONS_TIMER )
    {
        if (NotificationsController.i == null)
            return;
        NotificationsController.i.ShowNotification(new DCL.NotificationModel.Model
        {
            message = message,
            type = DCL.NotificationModel.Type.GENERIC,
            timer = timer,
            destroyOnFinish = true
        });
    }

    public static long ConvertToMilisecondsTimestamp(DateTime value)
    {
        TimeSpan elapsedTime = value - Epoch;
        return (long) elapsedTime.TotalMilliseconds;
    }

    public static SceneMetricsModel GetSceneMetricsLimits(int parcelAmount)
    {
        SceneMetricsModel  cachedModel = new SceneMetricsModel();

        float log = Mathf.Log(parcelAmount + 1, 2);
        float lineal = parcelAmount;

        cachedModel.triangles = (int) (lineal * SceneMetricsCounter.LimitsConfig.triangles);
        cachedModel.bodies = (int) (lineal * SceneMetricsCounter.LimitsConfig.bodies);
        cachedModel.entities = (int) (lineal * SceneMetricsCounter.LimitsConfig.entities);
        cachedModel.materials = (int) (log * SceneMetricsCounter.LimitsConfig.materials);
        cachedModel.textures = (int) (log * SceneMetricsCounter.LimitsConfig.textures);
        cachedModel.meshes = (int) (log * SceneMetricsCounter.LimitsConfig.meshes);
        cachedModel.sceneHeight = (int) (log * SceneMetricsCounter.LimitsConfig.height);

        return cachedModel;
    }

    public static Manifest CreateManifestFromProjectDataAndScene(ProjectData data, WebBuilderScene scene)
    {
        Manifest manifest = new Manifest();
        manifest.version = BIWSettings.MANIFEST_VERSION;
        manifest.project = data;
        manifest.scene = scene;

        manifest.project.scene_id = manifest.scene.id;
        return manifest;
    }

    public static Manifest CreateManifestFromProject(ProjectData projectData)
    {
        Manifest manifest = new Manifest();
        manifest.version = BIWSettings.MANIFEST_VERSION;
        manifest.project = projectData;
        manifest.scene = CreateEmtpyBuilderScene(projectData.rows, projectData.cols);

        manifest.project.scene_id = manifest.scene.id;
        return manifest;
    }

    //We create the scene the same way as the current builder do, so we ensure the compatibility between both builders
    private static WebBuilderScene CreateEmtpyBuilderScene(int rows, int cols)
    {
        Dictionary<string, BuilderEntity> entities = new Dictionary<string, BuilderEntity>();
        Dictionary<string, BuilderComponent> components = new Dictionary<string, BuilderComponent>();
        Dictionary<string, SceneObject> assets = new Dictionary<string, SceneObject>();
        
        // We get the asset
        var floorAsset = CreateFloorSceneObject();
        assets.Add(floorAsset.id,floorAsset);
        
        // We create the ground
        BuilderGround ground = new BuilderGround();
        ground.assetId = floorAsset.id;
        ground.componentId = Guid.NewGuid().ToString();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                // We create the entity for the ground
                BuilderEntity entity = new BuilderEntity();
                entity.id = Guid.NewGuid().ToString();
                entity.disableGizmos = true;
                entity.name = "entity"+x+y;
        
                // We need a transform for the entity so we create it
                BuilderComponent transformComponent = new BuilderComponent();
                transformComponent.id = Guid.NewGuid().ToString();
                transformComponent.type = "Transform";
        
                // We create the transform data
                TransformComponent entityTransformComponentModel = new TransformComponent();
                entityTransformComponentModel.position = new Vector3(8+(16*x), 0, 8+(16*y));
                entityTransformComponentModel.rotation = new ProtocolV2.QuaternionRepresentation(Quaternion.identity);
                entityTransformComponentModel.scale = Vector3.one;
        
                transformComponent.data = entityTransformComponentModel;
                entity.components.Add(transformComponent.id);
                if(!components.ContainsKey(transformComponent.id))
                    components.Add(transformComponent.id,transformComponent);
        
                // We create the GLTFShape component
                BuilderComponent gltfShapeComponent = new BuilderComponent();
                gltfShapeComponent.id = ground.componentId;
                gltfShapeComponent.type = "GLTFShape";
        
                LoadableShape.Model model = new GLTFShape.Model();
                model.assetId = floorAsset.id;
                gltfShapeComponent.data = model;
        
                entity.components.Add(ground.componentId);
                if(!components.ContainsKey(gltfShapeComponent.id))
                    components.Add(gltfShapeComponent.id,gltfShapeComponent);

                // Finally, we add the entity to the list
                entities.Add(entity.id,entity);
            }
        }

        WebBuilderScene scene = new WebBuilderScene
        {
            id = Guid.NewGuid().ToString(),
            entities = entities,
            components =  components,
            assets = assets,
            limits = GetSceneMetricsLimits(rows*cols),
            metrics = new SceneMetricsModel(),
            ground = ground
        };

        return scene;
    }

    public static Manifest CreateEmptyDefaultBuilderManifest(Vector2Int size, string landCoordinates)
    {
        Manifest manifest = new Manifest();
        manifest.version = BIWSettings.MANIFEST_VERSION;

        //We create a new project data for the scene
        ProjectData projectData = new ProjectData();
        projectData.id = Guid.NewGuid().ToString();
        projectData.eth_address = UserProfile.GetOwnUserProfile().ethAddress;
        projectData.title = "Builder " + landCoordinates;
        projectData.description = "Scene created from the explorer builder";
        projectData.creation_coords = landCoordinates;
        projectData.rows = size.x;
        projectData.cols = size.y;
        projectData.updated_at = DateTime.Now;
        projectData.created_at = DateTime.Now;
        projectData.thumbnail = "thumbnail.png";

        //We create an empty scene
        manifest.scene = CreateEmtpyBuilderScene(size.x, size.y);

        projectData.scene_id = manifest.scene.id;
        manifest.project = projectData;
        return manifest;
    }

    public static LandRole GetLandOwnershipType(List<LandWithAccess> lands, IParcelScene scene)
    {
        LandWithAccess filteredLand = lands.FirstOrDefault(land => scene.sceneData.basePosition == land.baseCoords);
        return GetLandOwnershipType(filteredLand);
    }

    public static LandRole GetLandOwnershipType(LandWithAccess land)
    {
        if (land != null)
            return land.role;
        return LandRole.OWNER;
    }

    public static Vector3 SnapFilterEulerAngles(Vector3 vectorToFilter, float degrees)
    {
        vectorToFilter.x = ClosestNumber(vectorToFilter.x, degrees);
        vectorToFilter.y = ClosestNumber(vectorToFilter.y, degrees);
        vectorToFilter.z = ClosestNumber(vectorToFilter.z, degrees);

        return vectorToFilter;
    }

    static float ClosestNumber(float n, float m)
    {
        // find the quotient 
        int q = Mathf.RoundToInt(n / m);

        // 1st possible closest number 
        float n1 = m * q;

        // 2nd possible closest number 
        float n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));

        // if true, then n1 is the required closest number 
        if (Math.Abs(n - n1) < Math.Abs(n - n2))
            return n1;

        // else n2 is the required closest number 
        return n2;
    }

    public static Vector2Int GetSceneSize(IParcelScene parcelScene) { return GetSceneSize(parcelScene.sceneData.parcels); }

    public static bool HasSquareSize(LandWithAccess land)
    {
        Vector2Int size = GetSceneSize(land.parcels);

        for (int x = land.baseCoords.x; x < size.x; x++)
        {
            bool found = false;
            foreach (Vector2Int parcel in land.parcels)
            {
                if (parcel.x == x)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }

        for (int y = land.baseCoords.y; y < size.x; y++)
        {
            bool found = false;
            foreach (Vector2Int parcel in land.parcels)
            {
                if (parcel.y == y)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }

        return true;
    }

    public static Vector2Int GetSceneSize(Vector2Int[] parcels)
    {
        int minX = Int32.MaxValue;
        int maxX = Int32.MinValue;
        int minY = Int32.MaxValue;
        int maxY = Int32.MinValue;

        foreach (var parcel in parcels)
        {
            if (parcel.x > maxX)
                maxX = parcel.x;
            if (parcel.x < minX)
                minX = parcel.x;

            if (parcel.y > maxY)
                maxY = parcel.y;
            if (parcel.y < minY)
                minY = parcel.y;
        }

        int sizeX = maxX - minX + 1;
        int sizeY = maxY - minY + 1;
        return new Vector2Int(sizeX, sizeY);
    }

    public static Vector3 CalculateUnityMiddlePoint(IParcelScene parcelScene)
    {
        Vector3 position;

        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        if (parcelScene?.sceneData == null || parcelScene?.sceneData.parcels == null)
            return Vector3.zero;

        foreach (Vector2Int vector in parcelScene?.sceneData.parcels)
        {
            totalX += vector.x;
            totalZ += vector.y;
            if (vector.x < minX)
                minX = vector.x;
            if (vector.y < minY)
                minY = vector.y;
            if (vector.x > maxX)
                maxX = vector.x;
            if (vector.y > maxY)
                maxY = vector.y;
        }

        float centerX = totalX / parcelScene.sceneData.parcels.Length + 0.5f;
        float centerZ = totalZ / parcelScene.sceneData.parcels.Length + 0.5f;

        position.x = centerX;
        position.y = totalY;
        position.z = centerZ;

        Vector3 scenePosition = Utils.GridToWorldPosition(centerX, centerZ);
        position = PositionUtils.WorldToUnityPosition(scenePosition);

        return position;
    }

    public static SceneObject CreateFloorSceneObject()
    {
        SceneObject floorSceneObject = new SceneObject();
        floorSceneObject.id = BIWSettings.FLOOR_ID;

        floorSceneObject.model = BIWSettings.FLOOR_MODEL;
        floorSceneObject.name = BIWSettings.FLOOR_NAME;
        floorSceneObject.asset_pack_id = BIWSettings.FLOOR_ASSET_PACK_ID;
        floorSceneObject.thumbnail = BIWSettings.FLOOR_ASSET_THUMBNAIL;
        floorSceneObject.category = BIWSettings.FLOOR_CATEGORY;
        
        floorSceneObject.tags = new List<string>();
        floorSceneObject.tags.Add("genesis");
        floorSceneObject.tags.Add("city");
        floorSceneObject.tags.Add("town");
        floorSceneObject.tags.Add("ground");

        floorSceneObject.contents = new Dictionary<string, string>();

        floorSceneObject.contents.Add(BIWSettings.FLOOR_GLTF_KEY, BIWSettings.FLOOR_GLTF_VALUE);
        floorSceneObject.contents.Add(BIWSettings.FLOOR_TEXTURE_KEY, BIWSettings.FLOOR_TEXTURE_VALUE);
        floorSceneObject.contents.Add(BIWSettings.FLOOR_THUMBNAIL_KEY, BIWSettings.FLOOR_THUMBNAIL_VALUE);

        floorSceneObject.metrics = new SceneObject.ObjectMetrics();

        return floorSceneObject;
    }
    
    public static CatalogItem CreateFloorCatalogItem()
    {
        CatalogItem floorSceneObject = new CatalogItem();
        floorSceneObject.id = BIWSettings.FLOOR_ID;

        floorSceneObject.model = BIWSettings.FLOOR_MODEL;
        floorSceneObject.name = BIWSettings.FLOOR_NAME;
        floorSceneObject.assetPackName = BIWSettings.FLOOR_ASSET_PACK_NAME;
        floorSceneObject.thumbnailURL = BIWSettings.FLOOR_ASSET_THUMBNAIL;

        floorSceneObject.tags = new List<string>();
        floorSceneObject.tags.Add("genesis");
        floorSceneObject.tags.Add("city");
        floorSceneObject.tags.Add("town");
        floorSceneObject.tags.Add("ground");
        
        floorSceneObject.contents = new Dictionary<string, string>();

        floorSceneObject.contents.Add(BIWSettings.FLOOR_GLTF_KEY, BIWSettings.FLOOR_GLTF_VALUE);
        floorSceneObject.contents.Add(BIWSettings.FLOOR_TEXTURE_KEY, BIWSettings.FLOOR_TEXTURE_VALUE);
        floorSceneObject.contents.Add(BIWSettings.FLOOR_THUMBNAIL_KEY, BIWSettings.FLOOR_THUMBNAIL_VALUE);

        floorSceneObject.metrics = new SceneObject.ObjectMetrics();

        return floorSceneObject;
    }

    public static Dictionary<string, string> ConvertMappingsToDictionary(ContentServerUtils.MappingPair[] contents)
    {
        Dictionary<string, string> mappingDict = new Dictionary<string, string>();

        foreach (ContentServerUtils.MappingPair mappingPair in contents)
        {
            mappingDict.Add(mappingPair.file, mappingPair.hash);
        }

        return mappingDict;
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        //Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = camera.ScreenToViewportPoint(screenPosition1);
        var v2 = camera.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();

        bounds.SetMinMax(min, max);
        return bounds;
    }

    public static bool IsWithinSelectionBounds(Transform transform, Vector3 lastClickMousePosition, Vector3 mousePosition) { return IsWithinSelectionBounds(transform.position, lastClickMousePosition, mousePosition); }

    public static bool IsWithinSelectionBounds(Vector3 point, Vector3 lastClickMousePosition, Vector3 mousePosition)
    {
        Camera camera = Camera.main;
        var viewPortBounds = GetViewportBounds(camera, lastClickMousePosition, mousePosition);
        return viewPortBounds.Contains(camera.WorldToViewportPoint(point));
    }

    public static bool IsBoundInsideCamera(Bounds bound)
    {
        Vector3[] points = { bound.max, bound.center, bound.min };
        return IsPointInsideCamera(points);
    }

    public static bool IsPointInsideCamera(Vector3[] points)
    {
        foreach (Vector3 point in points)
        {
            if (IsPointInsideCamera(point))
                return true;
        }
        return false;
    }

    public static bool IsPointInsideCamera(Vector3 point)
    {
        Vector3 topRight = new Vector3(Screen.width, Screen.height, 0);
        var viewPortBounds = GetViewportBounds(Camera.main, Vector3.zero, topRight);
        return viewPortBounds.Contains(Camera.main.WorldToViewportPoint(point));
    }

    public static void CopyGameObjectStatus(GameObject gameObjectToCopy, GameObject gameObjectToReceive, bool copyParent = true, bool localRotation = true)
    {
        if (copyParent)
            gameObjectToReceive.transform.SetParent(gameObjectToCopy.transform.parent);

        gameObjectToReceive.transform.position = gameObjectToCopy.transform.position;

        if (localRotation)
            gameObjectToReceive.transform.localRotation = gameObjectToCopy.transform.localRotation;
        else
            gameObjectToReceive.transform.rotation = gameObjectToCopy.transform.rotation;

        gameObjectToReceive.transform.localScale = gameObjectToCopy.transform.lossyScale;
    }

    public static bool IsPointerOverMaskElement(LayerMask mask)
    {
        RaycastHit hitInfo;
        UnityEngine.Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(mouseRay, out hitInfo, 5555, mask);
    }

    public static bool IsPointerOverUIElement(Vector3 mousePosition)
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 2;
    }

    public static bool IsPointerOverUIElement() { return IsPointerOverUIElement(Input.mousePosition); }

    public static string ConvertEntityToJSON(IDCLEntity entity)
    {
        EntityData builderInWorldEntityData = new EntityData();
        builderInWorldEntityData.entityId = entity.entityId;

        var components = entity.scene.componentsManagerLegacy.GetComponentsDictionary(entity);

        if (components != null)
        {
            foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> keyValuePair in components)
            {
                if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
                {
                    EntityData.TransformComponent entityComponentModel = new EntityData.TransformComponent();

                    entityComponentModel.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position, entity.scene);
                    entityComponentModel.rotation = entity.gameObject.transform.localRotation.eulerAngles;
                    entityComponentModel.scale = entity.gameObject.transform.localScale;

                    builderInWorldEntityData.transformComponent = entityComponentModel;
                }
                else
                {
                    ProtocolV2.GenericComponent entityComponentModel = new ProtocolV2.GenericComponent();
                    entityComponentModel.componentId = (int)keyValuePair.Key;
                    entityComponentModel.data = keyValuePair.Value.GetModel();

                    builderInWorldEntityData.components.Add(entityComponentModel);
                }
            }
        }

        var sharedComponents = entity.scene.componentsManagerLegacy.GetSharedComponentsDictionary(entity);

        if (sharedComponents != null)
        {
            foreach (KeyValuePair<Type, ISharedComponent> keyValuePair in sharedComponents)
            {
                if (keyValuePair.Value.GetClassId() == (int)CLASS_ID.NFT_SHAPE)
                {
                    EntityData.NFTComponent nFTComponent = new EntityData.NFTComponent();
                    NFTShape.Model model = (NFTShape.Model)keyValuePair.Value.GetModel();

                    nFTComponent.id = keyValuePair.Value.id;
                    nFTComponent.color = new ColorRepresentation(model.color);
                    nFTComponent.assetId = model.assetId;
                    nFTComponent.src = model.src;
                    nFTComponent.style = model.style;

                    builderInWorldEntityData.nftComponent = nFTComponent;
                }
                else
                {
                    ProtocolV2.GenericComponent entityComponentModel = new ProtocolV2.GenericComponent();
                    entityComponentModel.componentId = keyValuePair.Value.GetClassId();
                    entityComponentModel.data = keyValuePair.Value.GetModel();
                    entityComponentModel.classId = keyValuePair.Value.id;

                    builderInWorldEntityData.sharedComponents.Add(entityComponentModel);
                }
            }
        }


        return JsonConvert.SerializeObject(builderInWorldEntityData);
    }

    public static EntityData ConvertJSONToEntityData(string json) { return JsonConvert.DeserializeObject<EntityData>(json); }

    public static List<BIWEntity> RemoveGroundEntities(List<BIWEntity> entityList)
    {
        List<BIWEntity> newList = new List<BIWEntity>();

        foreach (BIWEntity entity in entityList)
        {
            if (entity.isFloor)
                continue;

            newList.Add(entity);
        }

        return newList;
    }

    public static List<BIWEntity> FilterEntitiesBySmartItemComponentAndActions(List<BIWEntity> entityList)
    {
        List<BIWEntity> newList = new List<BIWEntity>();

        foreach (BIWEntity entity in entityList)
        {
            if (!entity.HasSmartItemComponent() || !entity.HasSmartItemActions())
                continue;

            newList.Add(entity);
        }

        return newList;
    }

    public static void CopyRectTransform(RectTransform original, RectTransform rectTransformToCopy)
    {
        original.anchoredPosition = rectTransformToCopy.anchoredPosition;
        original.anchorMax = rectTransformToCopy.anchorMax;
        original.anchorMin = rectTransformToCopy.anchorMin;
        original.offsetMax = rectTransformToCopy.offsetMax;
        original.offsetMin = rectTransformToCopy.offsetMin;
        original.sizeDelta = rectTransformToCopy.sizeDelta;
        original.pivot = rectTransformToCopy.pivot;
    }

    public static IWebRequestAsyncOperation MakeGetCall(string url, Promise<string> callPromise, Dictionary<string, string> headers)
    {
        headers["Cache-Control"] = "no-cache";
        var asyncOperation = Environment.i.platform.webRequest.Get(
            url: url,
            OnSuccess: (webRequestResult) =>
            {
                byte[] byteArray = webRequestResult.GetResultData();
                string result = System.Text.Encoding.UTF8.GetString(byteArray);
                callPromise?.Resolve(result);
            },
            OnFail: (webRequestResult) =>
            {
                try
                {
                    byte[] byteArray = webRequestResult.GetResultData();
                    string result = System.Text.Encoding.UTF8.GetString(byteArray);
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(result);
                    callPromise?.Resolve(result);
                }
                catch (Exception e)
                {
                    Debug.Log(webRequestResult.webRequest.error);
                    callPromise.Reject(webRequestResult.webRequest.error);
                }
            },
            headers: headers);
        
        return asyncOperation;
    }

    public static IWebRequestAsyncOperation MakeGetTextureCall(string url, Promise<Texture2D> callPromise)
    {
        var asyncOperation = Environment.i.platform.webRequest.GetTexture(
            url: url,
            OnSuccess: (webRequestResult) =>
            {
                callPromise.Resolve(DownloadHandlerTexture.GetContent(webRequestResult.webRequest));
            },
            OnFail: (webRequestResult) =>
            {
                callPromise.Reject(webRequestResult.webRequest.error);
            });

        return asyncOperation;
    }

    public static void ConfigureEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        eventTrigger.triggers.Add(entry);
    }

    public static void RemoveEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType) { eventTrigger.triggers.RemoveAll(x => x.eventID == eventType); }
}