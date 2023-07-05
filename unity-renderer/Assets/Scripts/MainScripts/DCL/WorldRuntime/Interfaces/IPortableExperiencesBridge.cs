using System.Collections.Generic;

namespace DCL.WorldRuntime.PortableExperiences
{
    public interface IPortableExperiencesBridge
    {
        void SetDisabledPortableExperiences(IEnumerable<string> pxIds);
        void DestroyPortableExperience(string pxId);
    }
}
