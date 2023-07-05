namespace DCL.WorldRuntime.PortableExperiences
{
    public interface IPortableExperiencesController : IService
    {
        void EnablePortableExperience(string pxId);
        void DisablePortableExperience(string pxId);
        void DestroyPortableExperience(string pxId);
    }
}
