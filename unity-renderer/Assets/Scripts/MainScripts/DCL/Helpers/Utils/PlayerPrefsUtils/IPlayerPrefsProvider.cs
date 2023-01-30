namespace DCL.Helpers
{
    internal interface IPlayerPrefsProvider
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
}
