using DCL;
using DCL.Components;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DCL.Configuration;
using static ProtocolV2;
using Environment = DCL.Environment;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static partial class BuilderInWorldUtils
{
    public static CatalogItem CreateFloorSceneObject()
    {
        CatalogItem floorSceneObject = new CatalogItem();
        floorSceneObject.id = BuilderInWorldSettings.FLOOR_ID;

        floorSceneObject.model = BuilderInWorldSettings.FLOOR_MODEL;
        floorSceneObject.name = BuilderInWorldSettings.FLOOR_NAME;

        floorSceneObject.contents = new Dictionary<string, string>();

        floorSceneObject.contents.Add(BuilderInWorldSettings.FLOOR_GLTF_KEY, BuilderInWorldSettings.FLOOR_GLTF_VALUE);
        floorSceneObject.contents.Add(BuilderInWorldSettings.FLOOR_TEXTURE_KEY, BuilderInWorldSettings.FLOOR_TEXTURE_VALUE);

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

    public static bool IsWithInSelectionBounds(Transform transform, Vector3 lastClickMousePosition, Vector3 mousePosition)
    {
        return IsWithInSelectionBounds(transform.position, lastClickMousePosition, mousePosition);
    }

    public static bool IsWithInSelectionBounds(Vector3 point, Vector3 lastClickMousePosition, Vector3 mousePosition)
    {
        Camera camera = Camera.main;
        var viewPortBounds = GetViewportBounds(camera, lastClickMousePosition, mousePosition);
        return viewPortBounds.Contains(camera.WorldToViewportPoint(point));
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

    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(Input.mousePosition);
    }

    public static string ConvertEntityToJSON(DecentralandEntity entity)
    {
        EntityData builderInWorldEntityData = new EntityData();
        builderInWorldEntityData.entityId = entity.entityId;


        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
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

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePair in entity.GetSharedComponents())
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

    public static EntityData ConvertJSONToEntityData(string json)
    {
        return JsonConvert.DeserializeObject<EntityData>(json);
    }

    public static List<DCLBuilderInWorldEntity> FilterEntitiesBySmartItemComponentAndActions(List<DCLBuilderInWorldEntity> entityList)
    {
        List<DCLBuilderInWorldEntity> newList = new List<DCLBuilderInWorldEntity>();

        foreach (DCLBuilderInWorldEntity entity in entityList)
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
}