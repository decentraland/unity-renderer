using System;
using DCL.Builder;

namespace DCL.Builder
{
    internal interface ISectionFactory
    {
        SectionBase GetSectionController(SectionId id);
    }

    internal class SectionFactory : ISectionFactory
    {
        SectionBase ISectionFactory.GetSectionController(SectionId id)
        {
            SectionBase result = null;
            switch (id)
            {
                case SectionId.SCENES:
                    result = new SectionPlacesController();
                    break;
                case SectionId.PROJECTS:
                    result = new SectionProjectController();
                    break;
                case SectionId.LAND:
                    result = new SectionLandController();
                    break;
                case SectionId.SETTINGS_PROJECT_GENERAL:
                    result = new SectionPlaceGeneralSettingsController();
                    break;
                case SectionId.SETTINGS_PROJECT_CONTRIBUTORS:
                    result = new SectionPlaceContributorsSettingsController();
                    break;
                case SectionId.SETTINGS_PROJECT_ADMIN:
                    result = new SectionPlaceAdminsSettingsController();
                    break;
            }

            return result;
        }
    }
}