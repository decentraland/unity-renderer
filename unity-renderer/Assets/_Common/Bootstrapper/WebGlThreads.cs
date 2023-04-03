#if UNITY_WEBGL
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class WebGlThreads
{
    static WebGlThreads()
    {
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.threadsSupport = true;
        // PlayerSettings.WebGL.memorySize = 2048;
    }
}
#endif
