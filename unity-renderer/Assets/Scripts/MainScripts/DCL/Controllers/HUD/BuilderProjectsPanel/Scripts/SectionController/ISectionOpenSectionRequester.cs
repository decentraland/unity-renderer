using System;

internal interface ISectionOpenSectionRequester
{
    event Action<SectionId> OnRequestOpenSection;
}
