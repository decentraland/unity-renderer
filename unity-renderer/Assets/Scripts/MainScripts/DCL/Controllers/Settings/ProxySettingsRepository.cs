using System;

namespace DCL.SettingsCommon
{
    public class ProxySettingsRepository<T> : ISettingsRepository<T> where T : struct
    {
        private readonly ISettingsRepository<T> latestRepository;
        private readonly ISettingsRepository<T> fallbackRepository;

        public event Action<T> OnChanged
        {
            add => latestRepository.OnChanged += value;
            remove => latestRepository.OnChanged -= value;
        }

        public T Data
        {
            get
            {
                if (!latestRepository.HasAnyData() && fallbackRepository.HasAnyData())
                    latestRepository.Apply(fallbackRepository.Data);
                return latestRepository.Data;
            }
        }

        public ProxySettingsRepository(ISettingsRepository<T> latestRepository,
            ISettingsRepository<T> fallbackRepository)
        {
            this.latestRepository = latestRepository;
            this.fallbackRepository = fallbackRepository;
        }

        public void Apply(T settings) => latestRepository.Apply(settings);

        public void Reset() => latestRepository.Reset();

        public void Save() => latestRepository.Save();

        public bool HasAnyData() => latestRepository.HasAnyData();
    }
}