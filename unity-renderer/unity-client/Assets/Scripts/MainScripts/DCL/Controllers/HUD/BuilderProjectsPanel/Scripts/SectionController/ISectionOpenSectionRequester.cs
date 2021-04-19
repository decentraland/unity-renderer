using System;

internal interface ISectionOpenSectionRequester
{
    event Action<SectionsController.SectionId> OnRequestOpenSection;
}
