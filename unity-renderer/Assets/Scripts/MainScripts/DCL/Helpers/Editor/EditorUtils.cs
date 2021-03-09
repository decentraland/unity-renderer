using UnityEditor;
using UnityEngine;

public static class EditorUtils
{
    [MenuItem("Decentraland/Clear Player Prefs")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
