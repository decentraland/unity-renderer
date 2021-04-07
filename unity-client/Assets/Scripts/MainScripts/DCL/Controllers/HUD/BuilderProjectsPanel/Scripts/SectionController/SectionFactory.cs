using System;

internal interface ISectionFactory
{
    SectionBase GetSectionController(SectionsController.SectionId id);
}

internal class SectionFactory : ISectionFactory
{
    SectionBase ISectionFactory.GetSectionController(SectionsController.SectionId id)
    {
        SectionBase result = null;
        switch (id)
        {
            case SectionsController.SectionId.SCENES_MAIN:
                result = new SectionScenesController();
                break;
            case SectionsController.SectionId.SCENES_DEPLOYED:
                result = new SectionDeployedScenesController();
                break;
            case SectionsController.SectionId.SCENES_PROJECT:
                result = new SectionProjectScenesController();
                break;
            case SectionsController.SectionId.LAND:
                break;
            case SectionsController.SectionId.SETTINGS_PROJECT_GENERAL:
                result = new SectionSceneGeneralSettingsController();
                break;
            case SectionsController.SectionId.SETTINGS_PROJECT_CONTRIBUTORS:
                result = new SectionSceneContributorsSettingsController();
                break;
            case SectionsController.SectionId.SETTINGS_PROJECT_ADMIN:
                result = new SectionSceneAdminsSettingsController();
                break;
        }

        return result;
    }
}
