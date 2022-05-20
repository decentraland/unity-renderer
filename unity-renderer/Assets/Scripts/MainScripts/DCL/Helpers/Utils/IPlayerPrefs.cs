namespace DCL.Helpers
{
    public interface IPlayerPrefs
    {
        bool ContainsKey(string key);
        string GetString(string key, string defaultValue = null);
        bool GetBool(string key, bool defaultValue = false);
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0);
        void Set(string key, string value);
        void Set(string key, int value);
        void Set(string key, bool value);
        void Set(string key, float value);
        void Save();
    }
}
