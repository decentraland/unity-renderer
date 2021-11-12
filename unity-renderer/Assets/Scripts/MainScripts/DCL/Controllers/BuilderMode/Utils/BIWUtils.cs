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
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    public static void ShowGenericNotification(string message, DCL.NotificationModel.Type type = DCL.NotificationModel.Type.GENERIC, float timer = BIWSettings.LAND_NOTIFICATIONS_TIMER )
    {
        if(NotificationsController.i == null)
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

    public static Manifest CreateManifestFromProject(ProjectData projectData)
    {
        Manifest manifest = new Manifest();
        manifest.version = 10;
        manifest.project = projectData;
        manifest.scene = CreateEmtpyBuilderScene(projectData.rows * projectData.cols);

        manifest.project.scene_id = manifest.scene.id;
        return manifest;
    }

    //We create the scene the same way as the current builder do, so we ensure the compatibility between both builders
    private static BuilderScene CreateEmtpyBuilderScene(int parcelsAmount)
    {
        BuilderGround ground = new BuilderGround();
        ground.assetId = BIWSettings.FLOOR_ID;
        ground.componentId = Guid.NewGuid().ToString();
        
        BuilderScene scene = new BuilderScene
        {
            id = Guid.NewGuid().ToString(), 
            limits = GetSceneMetricsLimits(parcelsAmount),
            metrics = new SceneMetricsModel(),
            ground = ground
        };

        return scene;
    }
    
    public static Manifest CreateEmptyDefaultBuilderManifest(string landCoordinates)
    {
        Manifest manifest = new Manifest();
        return manifest;
    }

    public static LandRole GetLandOwnershipType(List<LandWithAccess> lands, ParcelScene scene)
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

    public static Vector2Int GetSceneSize(ParcelScene parcelScene)
    {
        int minX = Int32.MaxValue;
        int maxX = Int32.MinValue;
        int minY = Int32.MaxValue;
        int maxY = Int32.MinValue;

        foreach (var parcel in parcelScene.sceneData.parcels)
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

        if (parcelScene.sceneData == null || parcelScene.sceneData.parcels == null)
            return Vector3.zero;

        foreach (Vector2Int vector in parcelScene.sceneData.parcels)
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

        float centerX = totalX / parcelScene.sceneData.parcels.Length;
        float centerZ = totalZ / parcelScene.sceneData.parcels.Length;

        position.x = centerX;
        position.y = totalY;
        position.z = centerZ;

        position = WorldStateUtils.ConvertScenePositionToUnityPosition(parcelScene);

        position.x += ParcelSettings.PARCEL_SIZE / 2;
        position.z += ParcelSettings.PARCEL_SIZE / 2;

        return position;
    }

    public static CatalogItem CreateFloorSceneObject()
    {
        CatalogItem floorSceneObject = new CatalogItem();
        floorSceneObject.id = BIWSettings.FLOOR_ID;

        floorSceneObject.model = BIWSettings.FLOOR_MODEL;
        floorSceneObject.name = BIWSettings.FLOOR_NAME;
        floorSceneObject.assetPackName = BIWSettings.FLOOR_ASSET_PACK_NAME;

        floorSceneObject.contents = new Dictionary<string, string>();

        floorSceneObject.contents.Add(BIWSettings.FLOOR_GLTF_KEY, BIWSettings.FLOOR_GLTF_VALUE);
        floorSceneObject.contents.Add(BIWSettings.FLOOR_TEXTURE_KEY, BIWSettings.FLOOR_TEXTURE_VALUE);

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


        foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> keyValuePair in entity.components)
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
                entityComponentModel.componentId = (int) keyValuePair.Key;
                entityComponentModel.data = keyValuePair.Value.GetModel();

                builderInWorldEntityData.components.Add(entityComponentModel);
            }
        }

        foreach (KeyValuePair<Type, ISharedComponent> keyValuePair in entity.sharedComponents)
        {
            if (keyValuePair.Value.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
            {
                EntityData.NFTComponent nFTComponent = new EntityData.NFTComponent();
                NFTShape.Model model = (NFTShape.Model) keyValuePair.Value.GetModel();

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
                Debug.Log(webRequestResult.webRequest.error);
                callPromise.Reject(webRequestResult.webRequest.error);
            },
            headers: headers);

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