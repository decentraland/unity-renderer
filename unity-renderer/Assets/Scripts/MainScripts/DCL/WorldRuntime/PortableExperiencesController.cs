namespace DCL.WorldRuntime.PortableExperiences
{
    public class PortableExperiencesController : IPortableExperiencesController
    {
        private readonly IPortableExperiencesBridge bridge;
        private readonly DataStore dataStore;

        private BaseCollection<string> disabledPortableExperienceIds => dataStore.world.disabledPortableExperienceIds;

        public PortableExperiencesController(IPortableExperiencesBridge bridge,
            DataStore dataStore)
        {
            this.bridge = bridge;
            this.dataStore = dataStore;
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public void EnablePortableExperience(string pxId)
        {
            disabledPortableExperienceIds.Remove(pxId);
            bridge.SetDisabledPortableExperiences(disabledPortableExperienceIds.Get());
        }

        public void DisablePortableExperience(string pxId)
        {
            if (!disabledPortableExperienceIds.Contains(pxId))
                disabledPortableExperienceIds.Add(pxId);
            bridge.SetDisabledPortableExperiences(disabledPortableExperienceIds.Get());
        }

        public void DestroyPortableExperience(string pxId) =>
            bridge.DestroyPortableExperience(pxId);
    }
}
