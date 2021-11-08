using System;

namespace DCL.Builder
{
    internal interface ISectionOpenSectionRequester
    {
        event Action<SectionId> OnRequestOpenSection;
    }
}