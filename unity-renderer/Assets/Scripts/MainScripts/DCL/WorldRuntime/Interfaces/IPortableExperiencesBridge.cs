using System.Collections.Generic;

namespace DCL
{
    public interface IPortableExperiencesBridge
    {
        void SetDisabledPortableExperiences(IEnumerable<string> pxIds);
    }
}
