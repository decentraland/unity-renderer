using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebGLFileSaver
{

    [DllImport("__Internal")]
    private static extern void UNITY_SAVE(string content, string name, string MIMEType);
    
    [DllImport ("__Internal")]
    private static extern void UNITY_SAVE_BYTEARRAY(byte[] array, int byteLength, string name, string MIMEType);

    [DllImport("__Internal")]
    private static extern void init();

    [DllImport("__Internal")]
    private static extern bool UNITY_IS_SUPPORTED();

    static bool hasinit = false;

    public static void SaveFile(string content, string fileName, string MIMEType = "text/plain;charset=utf-8")
    {
       if (!CheckSupportAndInit()) return;

        UNITY_SAVE (content, fileName, MIMEType);
    }
    
    public static void SaveFile(byte[] content, string fileName, string MIMEType = "text/plain;charset=utf-8")
    {
        if (content == null)
        {
            Debug.LogError("null parameter passed for content byte array");
            return;
        }
        if (!CheckSupportAndInit()) return;

        UNITY_SAVE_BYTEARRAY (content, content.Length, fileName, MIMEType);
    }

    static bool CheckSupportAndInit()
    {
        if (Application.isEditor)
        {
            Debug.Log("Saving will not work in editor.");
            return false;
        }
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("Saving must be on a WebGL build.");
            return false;
        }

        CheckInit();

        if (!IsSavingSupported())
        { 
            Debug.LogWarning("Saving is not supported on this device.");
            return false;
        }
        return true;
    }

    static void CheckInit()
    {
        if (!hasinit)
        {
            init();
            hasinit = true;
        }
    }

    public static bool IsSavingSupported()
    {
        if (Application.isEditor)
        {
            Debug.Log("Saving will not work in editor.");
            return false;
        }
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("Saving must be on a WebGL build.");
            return false;
        }
        CheckInit();
        return UNITY_IS_SUPPORTED();
    }
}
