using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// MacOS doesn't include Python anymore since macOS 12.3 (and removes the previously-existent version)
// Emscripten build toolchain for WebGL builds on Unity 2020.3 depends on Python 2.7 or later. [WebGL builds on Unity 2021.2 and later require Python 3].
// Solution for building locally for WebGL on macOS:
// 1. Download Python 3+ at https://www.python.org/downloads/release/python-3105/
// 2. Have this PreBuildProcessing script in place to re-map Python path when building.
// https://forum.unity.com/threads/case-1412113-builderror-osx-12-3-and-unity-2020-3-constant-build-errors.1255419/#post-7993017
// https://answers.unity.com/questions/1893841/unity-2020328f1-webgl-build-failed-on-macos-monter.html

public class PreBuildProcessing : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;
    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_EDITOR && UNITY_EDITOR_OSX
        System.Environment.SetEnvironmentVariable("EMSDK_PYTHON", "/Library/Frameworks/Python.framework/Versions/3.10/bin/python3");
#endif

#if UNITY_WEBGL
        // Tip: ' --memoryprofiler ' argument can be added to log every memory enlargement in the console but it makes the app super slow
        PlayerSettings.WebGL.emscriptenArgs += "-s ALLOW_MEMORY_GROWTH=1 -s MAXIMUM_MEMORY=4GB -s ERROR_ON_UNDEFINED_SYMBOLS=0 ";
#endif
    }
}
