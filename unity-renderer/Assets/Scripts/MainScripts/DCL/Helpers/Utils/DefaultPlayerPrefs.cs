namespace DCL.Helpers
{
    public class DefaultPlayerPrefs : IPlayerPrefs
    {
        public bool ContainsKey(string key) => PlayerPrefsBridge.HasKey(key);

        public string GetString(string key, string defaultValue = null) =>
            PlayerPrefsBridge.GetString(key, defaultValue);

        public bool GetBool(string key, bool defaultValue = false) =>
            PlayerPrefsBridge.GetBool(key, defaultValue);

        public int GetInt(string key, int defaultValue = 0) =>
            PlayerPrefsBridge.GetInt(key, defaultValue);

        public float GetFloat(string key, float defaultValue = 0) =>
            PlayerPrefsBridge.GetFloat(key, defaultValue);

        public void Set(string key, string value) =>
            PlayerPrefsBridge.SetString(key, value);

        public void Set(string key, int value) =>
            PlayerPrefsBridge.SetInt(key, value);

        public void Set(string key, bool value) =>
            PlayerPrefsBridge.SetBool(key, value);

        public void Set(string key, float value) =>
            PlayerPrefsBridge.SetFloat(key, value);

        public void Save() => PlayerPrefsBridge.Save();
    }
}
