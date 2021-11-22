namespace DCL.SettingsCommon
{
    public interface IPlayerPrefsSettingsByKey
    {
        T GetEnum<T>(string fieldName, T defaultValue) where T : struct;
        bool GetBool(string fieldName, bool defaultValue);
        float GetFloat(string fieldName, float defaultValue);
        int GetInt(string fieldName, int defaultValue);
        string GetString(string fieldName, string defaultValue);
        void SetBool(string fieldName, bool value);
        void SetFloat(string fieldName, float value);
        void SetEnum<T>(string fieldName, T value) where T : struct;
        void SetInt(string fieldName, int value);
        void SetString(string fieldName, string value);
    }
}