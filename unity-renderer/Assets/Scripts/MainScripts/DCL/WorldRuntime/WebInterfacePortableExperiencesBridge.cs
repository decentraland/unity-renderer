using DCL.Interface;
using System.Collections.Generic;
using System.Linq;

namespace DCL.WorldRuntime.PortableExperiences
{
    public class WebInterfacePortableExperiencesBridge : IPortableExperiencesBridge
    {
        public void SetDisabledPortableExperiences(IEnumerable<string> pxIds) =>
            WebInterface.SetDisabledPortableExperiences(pxIds.ToArray());

        public void DestroyPortableExperience(string pxId) =>
            WebInterface.KillPortableExperience(pxId);
    }
}
