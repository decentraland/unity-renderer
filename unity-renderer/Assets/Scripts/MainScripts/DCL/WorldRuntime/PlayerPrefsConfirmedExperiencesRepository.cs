using DCL.Helpers;

namespace DCL.World.PortableExperiences
{
    public class PlayerPrefsConfirmedExperiencesRepository : IConfirmedExperiencesRepository
    {
        private const string KEY_PREFIX = "PortableExperiences.Confirmation.";

        private readonly IPlayerPrefs playerPrefs;

        public PlayerPrefsConfirmedExperiencesRepository(IPlayerPrefs playerPrefs)
        {
            this.playerPrefs = playerPrefs;
        }

        public bool Get(string pxId) =>
            playerPrefs.GetBool(GetKey(pxId), false);

        public void Set(string pxId, bool confirmed) =>
            playerPrefs.Set(GetKey(pxId), confirmed);

        public bool Contains(string pxId) =>
            playerPrefs.ContainsKey(GetKey(pxId));

        private string GetKey(string pxId) =>
            $"{KEY_PREFIX}{pxId}";
    }
}
