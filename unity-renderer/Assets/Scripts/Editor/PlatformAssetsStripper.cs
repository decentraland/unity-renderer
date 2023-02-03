using System.IO;
using UnityEditor;
using UnityEngine;

public static class PlatformAssetsStripper
{
    private static string DESKTOP_PATH => Application.dataPath + "/Desktop";
    private static string DESKTOP_HIDDEN_PATH = Application.dataPath + "/.Desktop";

    [MenuItem("Decentraland/Set Platform/Only WebGl")]
    public static void SetOnlyWebGL()
    {
        if (!Directory.Exists(DESKTOP_PATH))
        {
            Debug.Log($"Couldnt find folder: {DESKTOP_PATH}");
            return;
        }

        Directory.Move(DESKTOP_PATH, DESKTOP_HIDDEN_PATH);
        Directory.Move($"{DESKTOP_PATH}.meta", $"{DESKTOP_HIDDEN_PATH}.meta");
        AssetDatabase.Refresh();
    }

    [MenuItem("Decentraland/Set Platform/Only Desktop")]
    public static void SetOnlyDesktop()
    {
        //TODO remove WebGL specific once it's extracted.
        Debug.Log("Nothing to do until we subtract WebGL specifics from Common, Restoring All");
        RestoreAll();
    }

    [MenuItem("Decentraland/Set Platform/Restore All")]
    public static void RestoreAll()
    {
        if (!Directory.Exists(DESKTOP_HIDDEN_PATH))
        {
            Debug.Log($"Couldnt find folder: {DESKTOP_HIDDEN_PATH}");
            return;
        }

        Directory.Move(DESKTOP_HIDDEN_PATH, DESKTOP_PATH);
        Directory.Move($"{DESKTOP_HIDDEN_PATH}.meta", $"{DESKTOP_PATH}.meta");
        AssetDatabase.Refresh();
    }
}
