using UnityEngine;

public static class CommonScriptableObjects
{
    private static Vector3Variable playerUnityPositionValue;
    public static Vector3Variable playerUnityPosition => GetOrLoad(ref playerUnityPositionValue, "ScriptableObjects/PlayerUnityPosition");

    private static Vector3Variable playerUnityEulerAnglesValue;
    public static Vector3Variable playerUnityEulerAngles => GetOrLoad(ref playerUnityEulerAnglesValue, "ScriptableObjects/PlayerUnityEulerAngles");

    private static Vector3Variable playerUnityToWorldOffsetValue;
    public static Vector3Variable playerUnityToWorldOffset => GetOrLoad(ref playerUnityToWorldOffsetValue , "ScriptableObjects/PlayerUnityToWorldOffset");

    private static Vector2IntVariable playerCoordsValue;
    public static Vector2IntVariable playerCoords => GetOrLoad(ref playerCoordsValue , "ScriptableObjects/PlayerCoords");

    private static T GetOrLoad<T>(ref T variable, string path) where T : Object
    {
        if (variable == null)
        {
            variable = Resources.Load<T>(path);
        }
        return variable;
    }
}