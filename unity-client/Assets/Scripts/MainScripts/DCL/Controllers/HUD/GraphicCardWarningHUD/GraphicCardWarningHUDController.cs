
public class GraphicCardWarningHUDController : IHUD
{
    private const string GRAPHIC_CARD_MESSAGE = "Your machine is not using a dedicated graphics card to run Decentraland. This might lead to performance issues. Check your browser and OS configuration.";

    public GraphicCardWarningHUDController() { }

    public void SetVisibility(bool visible)
    {
        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActiveChanged;
        CommonScriptableObjects.rendererState.OnChange -= RendererStateChanged;

        if (!visible)
            return;

        if (!CommonScriptableObjects.tutorialActive.Get() && CommonScriptableObjects.rendererState)
        {
            TryShowNotification();
        }
        else
        {
            if (CommonScriptableObjects.tutorialActive)
                CommonScriptableObjects.tutorialActive.OnChange += TutorialActiveChanged;
            else
                CommonScriptableObjects.rendererState.OnChange += RendererStateChanged;
            
        }

    }

    private void TutorialActiveChanged(bool newState, bool oldState)
    {
        if (newState) return;

        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActiveChanged;
        TryShowNotification();
    }

    private void RendererStateChanged(bool newState, bool oldState)
    {
        if (!newState) return;

        CommonScriptableObjects.rendererState.OnChange -= RendererStateChanged;
        TryShowNotification();
    }

    private void TryShowNotification()
    {
        if (IsIntegratedGraphicCard())
        {
            NotificationsController.i.ShowNotification(new Notification.Model
            {
                buttonMessage = "Dismiss",
                destroyOnFinish = true,
                groupID = "GraphicCard",
                message = GRAPHIC_CARD_MESSAGE,
                timer = 0,
                type = NotificationFactory.Type.GRAPHIC_CARD_WARNING
            });
        }
    }

    private bool IsIntegratedGraphicCard() => DCL.Interface.WebInterface.GetGraphicCard().ToLower().Contains("intel");

    public void Dispose() { }
}
