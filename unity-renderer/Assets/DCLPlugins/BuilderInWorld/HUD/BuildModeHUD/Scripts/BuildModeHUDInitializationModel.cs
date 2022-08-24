using DCL.Builder;

public class BuildModeHUDInitializationModel
{
    public ITooltipController tooltipController;
    public ITooltipController feedbackTooltipController;
    public ISceneCatalogController sceneCatalogController;
    public IQuickBarController quickBarController;
    public IEntityInformationController entityInformationController;
    public IFirstPersonModeController firstPersonModeController;
    public IShortcutsController shortcutsController;
    public IDragAndDropSceneObjectController dragAndDropSceneObjectController;
    public IPublishBtnController publishBtnController;
    public IInspectorBtnController inspectorBtnController;
    public ICatalogBtnController catalogBtnController;
    public IInspectorController inspectorController;
    public ITopActionsButtonsController topActionsButtonsController;
    public IBuildModeConfirmationModalController buildModeConfirmationModalController;
    public ISaveHUDController saveHUDController;
    public INewProjectDetailController newProjectDetailsController;

    public void Dispose()
    {
        newProjectDetailsController.Dispose();
        saveHUDController.Dispose();
        buildModeConfirmationModalController.Dispose();
        topActionsButtonsController.Dispose();
        inspectorController.Dispose();
        catalogBtnController.Dispose();
        inspectorBtnController.Dispose();
        publishBtnController.Dispose();
        dragAndDropSceneObjectController.Dispose();
        shortcutsController.Dispose();
        firstPersonModeController.Dispose();
        entityInformationController.Dispose();
        quickBarController.Dispose();
        sceneCatalogController.Dispose();
        feedbackTooltipController.Dispose();
        tooltipController.Dispose();
    }
}