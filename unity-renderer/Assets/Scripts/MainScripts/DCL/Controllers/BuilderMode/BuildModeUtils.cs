using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static partial class BuildModeUtils
{

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
        var viewPortBounds = BuildModeUtils.GetViewportBounds(camera, lastClickMousePosition, mousePosition);
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
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(mouseRay, out hitInfo, 5555, mask);
    }

    public static bool IsPointerOverUIElement()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 1;
    }

}

