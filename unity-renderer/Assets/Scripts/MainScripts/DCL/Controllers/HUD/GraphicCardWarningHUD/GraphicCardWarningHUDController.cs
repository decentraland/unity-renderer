using DCL.Interface;
using DCL.NotificationModel;
using UnityEngine;

public class GraphicCardWarningHUDController : IHUD
{
    private readonly string warningMessage =
        "Your machine is not using a dedicated graphics card to run Decentraland. "
        + "This might lead to performance issues. Check your browser and OS configuration "
        + "and restart " + (Application.platform == RuntimePlatform.WebGLPlayer ? "your browser." : "the experience.");

    public void Dispose() { }

    public void SetVisibility(bool visible)
    {
        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActiveChanged;
        CommonScriptableObjects.rendererState.OnChange -= RendererStateChanged;

        if (!visible)
            return;

        if (!CommonScriptableObjects.tutorialActive.Get() && CommonScriptableObjects.rendererState)
            TryShowNotification();
        else
        {
            if (CommonScriptableObjects.tutorialActive)
                CommonScriptableObjects.tutorialActive.OnChange += TutorialActiveChanged;
            else
                CommonScriptableObjects.rendererState.OnChange += RendererStateChanged;
        }
    }

    private void TutorialActiveChanged(bool newState, bool _)
    {
        if (newState)
            return;

        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActiveChanged;
        TryShowNotification();
    }

    private void RendererStateChanged(bool newState, bool _)
    {
        if (!newState)
            return;

        CommonScriptableObjects.rendererState.OnChange -= RendererStateChanged;
        TryShowNotification();
    }

    private void TryShowNotification()
    {
        if (GraphicCardNotification.CanShowGraphicCardPopup() && IsIntegratedGraphicCard())
            NotificationsController.i.ShowNotification(
                new Model
                {
                    buttonMessage = "Dismiss",
                    destroyOnFinish = true,
                    groupID = "GraphicCard",
                    message = warningMessage,
                    timer = 0,
                    type = Type.GRAPHIC_CARD_WARNING,
                });

        bool IsIntegratedGraphicCard() =>
            WebInterface.GetGraphicCard().ToLower().Contains("intel");
    }
}
