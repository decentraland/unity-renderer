namespace DCL.PortableExperiences.Confirmation
{
    public interface IConfirmedExperiencesRepository
    {
        bool Get(string pxId);
        void Set(string pxId, bool confirmed);
        bool Contains(string pxId);
    }
}
