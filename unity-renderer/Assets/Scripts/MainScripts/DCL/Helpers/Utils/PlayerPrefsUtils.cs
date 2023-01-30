using System.Runtime.InteropServices;
using UnityEngine;

namespace DCL.Helpers
{
    public static class PlayerPrefsUtils
    {
        private static readonly PlayerPrefsProvider PROVIDER = Application.platform.Equals(RuntimePlatform.WebGLPlayer) && !Application.isEditor
            ? new PlayerPrefsProviderLocalStorage()
            : new PlayerPrefsProviderUnity();

        public static int GetInt(string key) =>
            PROVIDER.GetInt(key);

        public static int GetInt(string key, int defaultValue) =>
            PROVIDER.GetInt(key, defaultValue);

        public static bool GetBool(string key, bool defaultValue) =>
            PROVIDER.GetBool(key, defaultValue);

        public static void SetBool(string key, bool value)
        {
            PROVIDER.SetBool(key, value);
        }

        public static void SetInt(string key, int value)
        {
            try { PROVIDER.SetInt(key, value); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs int!"); }
        }

        public static bool HasKey(string key) =>
            PROVIDER.HasKey(key);

        public static string GetString(string key, string defaultValue = null) =>
            PROVIDER.GetString(key, defaultValue);

        public static void SetString(string key, string value)
        {
            try { PROVIDER.SetString(key, value); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs string!"); }
        }

        public static void Save()
        {
            PROVIDER.Save();
        }

        public static float GetFloat(string key, float defaultValue = 0f) =>
            PROVIDER.GetFloat(key, defaultValue);

        public static void SetFloat(string key, float value)
        {
            try { PROVIDER.SetFloat(key, value); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs float!"); }
        }
    }
}

internal interface PlayerPrefsProvider
{
    int GetInt(string key);

    int GetInt(string key, int defaultValue);

    void SetInt(string key, int value);

    bool GetBool(string key, bool defaultValue);

    void SetBool(string key, bool value);

    string GetString(string key, string defaultValue);

    void SetString(string key, string value);

    void Save();

    float GetFloat(string key, float defaultValue);

    void SetFloat(string key, float value);

    bool HasKey(string key);
}

public class PlayerPrefsProviderLocalStorage : PlayerPrefsProvider
{
    public int GetInt(string key) =>
        int.Parse(GetLocalStorageValue(key, "0"));

    public int GetInt(string key, int defaultValue) =>
        int.Parse(GetLocalStorageValue(key, defaultValue.ToString()));

    public void SetInt(string key, int value)
    {
        SaveStringToLocalStorage(key, value.ToString());
    }

    public bool GetBool(string key, bool defaultValue) =>
        GetInt(key, defaultValue ? 1 : 0) == 1;

    public void SetBool(string key, bool value) =>
        SetInt(key, value ? 1 : 0);

    public string GetString(string key, string defaultValue) =>
        GetLocalStorageValue(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue);

    public void SetString(string key, string value)
    {
        SaveStringToLocalStorage(key, value);
    }

    //Nothing to save when using local storage
    public void Save() { }

    public float GetFloat(string key, float defaultValue)
    {
        Debug.Log($"GETTING FLOAT FROM LOCAL STORAGE {float.Parse(GetLocalStorageValue(key, defaultValue.ToString()))}");
        return float.Parse(GetLocalStorageValue(key, defaultValue.ToString()));
    }

    public void SetFloat(string key, float value)
    {
        Debug.Log($"SETTING FLOAT IN LOCAL STORAGE {value.ToString()}");
        SaveStringToLocalStorage(key, value.ToString());
    }

    public bool HasKey(string key) =>
        HasKeyInLocalStorage(key) == 1;

    private string GetLocalStorageValue(string key, string defaultValue)
    {
        if (HasKey(key))
            return LoadStringFromLocalStorage(key);

        return defaultValue;
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SaveStringToLocalStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern string LoadStringFromLocalStorage(string key);

    [DllImport("__Internal")]
    private static extern int HasKeyInLocalStorage(string key);
#else
    private static void SaveStringToLocalStorage(string key, string value) { }

    private static string LoadStringFromLocalStorage(string key) =>
        "";

    private static int HasKeyInLocalStorage(string key) =>
        0;
#endif
}

public class PlayerPrefsProviderUnity : PlayerPrefsProvider
{
    public int GetInt(string key) =>
        PlayerPrefs.GetInt(key);

    public int GetInt(string key, int defaultValue) =>
        PlayerPrefs.GetInt(key, defaultValue);

    public void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public bool GetBool(string key, bool defaultValue) =>
        PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

    public void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public string GetString(string key, string defaultValue) =>
        PlayerPrefs.GetString(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue);

    public void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    public float GetFloat(string key, float defaultValue) =>
        PlayerPrefs.GetFloat(key, defaultValue);

    public void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public bool HasKey(string key) =>
        PlayerPrefs.HasKey(key);
}
