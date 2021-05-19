using UnityEditor;
using UnityEngine;

public static class EditorUtils
{
    [MenuItem("Decentraland/Clear Player Prefs")]
    public static void ClearAllPlayerPrefs() { PlayerPrefs.DeleteAll(); }
}

public static class CompilerOptions
{
    [InitializeOnLoadMethod]
    public static void SetProfilingFuncs()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        if ( target == BuildTarget.WebGL)
        {
            PlayerSettings.WebGL.emscriptenArgs = " --profiling-funcs ";
        }
    }
}