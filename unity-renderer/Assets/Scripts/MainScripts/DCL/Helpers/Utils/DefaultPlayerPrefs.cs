namespace DCL.Helpers
{
    public class DefaultPlayerPrefs : IPlayerPrefs
    {
        public bool ContainsKey(string key) => PlayerPrefsUtils.HasKey(key);

        public string GetString(string key, string defaultValue = null) =>
            PlayerPrefsUtils.GetString(key, defaultValue);

        public bool GetBool(string key, bool defaultValue = false) =>
            PlayerPrefsUtils.GetBool(key, defaultValue);

        public int GetInt(string key, int defaultValue = 0) =>
            PlayerPrefsUtils.GetInt(key, defaultValue);

        public float GetFloat(string key, float defaultValue = 0) =>
            PlayerPrefsUtils.GetFloat(key, defaultValue);

        public void Set(string key, string value) =>
            PlayerPrefsUtils.SetString(key, value);

        public void Set(string key, int value) =>
            PlayerPrefsUtils.SetInt(key, value);

        public void Set(string key, bool value) =>
            PlayerPrefsUtils.SetBool(key, value);

        public void Set(string key, float value) =>
            PlayerPrefsUtils.SetFloat(key, value);

        public void Save() => PlayerPrefsUtils.Save();
    }
}