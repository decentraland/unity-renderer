using System;

namespace DCL.SettingsCommon
{
    public interface ISettingsRepository<T> where T : struct
    {
        event Action<T> OnChanged;
        T Data { get; }
        void Apply(T settings);
        void Reset();
        void Save();
        bool HasAnyData();
    }
}