using DCL.Interface;
using System.Collections.Generic;
using System.Linq;

namespace DCL
{
    public class WebInterfacePortableExperiencesBridge : IPortableExperiencesBridge
    {
        public void SetDisabledPortableExperiences(IEnumerable<string> pxIds) =>
            WebInterface.SetDisabledPortableExperiences(pxIds.ToArray());
    }
}
