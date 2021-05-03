using System;

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
            case SectionId.SCENES_MAIN:
                result = new SectionScenesController();
                break;
            case SectionId.SCENES_DEPLOYED:
                result = new SectionDeployedScenesController();
                break;
            case SectionId.SCENES_PROJECT:
                result = new SectionProjectScenesController();
                break;
            case SectionId.LAND:
                result = new SectionLandController();
                break;
            case SectionId.SETTINGS_PROJECT_GENERAL:
                result = new SectionSceneGeneralSettingsController();
                break;
            case SectionId.SETTINGS_PROJECT_CONTRIBUTORS:
                result = new SectionSceneContributorsSettingsController();
                break;
            case SectionId.SETTINGS_PROJECT_ADMIN:
                result = new SectionSceneAdminsSettingsController();
                break;
        }

        return result;
    }
}
