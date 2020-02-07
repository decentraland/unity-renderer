using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class InitialStep : TutorialStep
{
    [SerializeField] [FormerlySerializedAs("wellcomeTooltip")] TutorialTooltip welcomeTooltip = null;
    [SerializeField] TutorialTooltip controlsTooltip = null;
    [SerializeField] TutorialTooltip cameraTooltip = null;
    [SerializeField] TutorialTooltip minimapTooltip = null;
    [SerializeField] GameObject claimNamePanel = null;

    private AvatarEditorHUDController avatarEditorHUD = null;

    private bool claimNamePanelClosed = false;
    private bool characterMoved = false;
    private bool characterTeleported = false;

    public override void OnStepStart()
    {
        base.OnStepStart();

        HUDController.i?.minimapHud.SetVisibility(false);
        HUDController.i?.expressionsHud.SetVisibility(false);
        TutorialController.i.SetChatVisible(false);

        DCLCharacterController.OnPositionSet += OnTeleport;
        DCLCharacterController.OnCharacterMoved += OnCharacterMove;

        if (HUDController.i != null && HUDController.i.avatarEditorHud != null)
        {
            avatarEditorHUD = HUDController.i.avatarEditorHud;
            avatarEditorHUD.SetVisibility(true);
            avatarEditorHUD.OnVisibilityChanged += OnAvatarEditorVisibilityChanged;
            claimNamePanelClosed = false;
        }
        else
        {
            claimNamePanelClosed = true;
        }
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();
        DCLCharacterController.OnPositionSet -= OnTeleport;
        DCLCharacterController.OnCharacterMoved -= OnCharacterMove;
    }

    public override IEnumerator OnStepExecute()
    {
        yield return ShowTooltip(welcomeTooltip);

        yield return new WaitUntil(() => claimNamePanelClosed);
        yield return WaitForSecondsWhenRenderingEnabled(3);

        yield return ShowTooltip(controlsTooltip, autoHide: false);
        characterMoved = false;
        characterTeleported = false;

#if UNITY_EDITOR
        if (DCLCharacterController.i == null)
        {
            characterMoved = true;
        }
#endif

        yield return new WaitUntil(() => characterMoved);
        yield return WaitIdleTime();
        HideTooltip(controlsTooltip);

        yield return WaitForSecondsWhenRenderingEnabled(3);
        yield return ShowTooltip(cameraTooltip);
        yield return WaitIdleTime();

        HUDController.i?.minimapHud.SetVisibility(true);
        yield return ShowTooltip(minimapTooltip);

#if UNITY_EDITOR
        if (TutorialController.i.debugRunTutorialOnStart)
        {
            characterTeleported = true;
        }
#endif

        yield return new WaitUntil(() => characterTeleported == true);

    }

    private void OnAvatarEditorVisibilityChanged(bool visible)
    {
        if (visible) return;

        if (avatarEditorHUD != null)
            avatarEditorHUD.OnVisibilityChanged -= OnAvatarEditorVisibilityChanged;

        claimNamePanel.SetActive(true);
    }

    private void OnTeleport(DCLCharacterPosition characterPosition)
    {
        characterTeleported = true;
    }

    private void OnCharacterMove(DCLCharacterPosition position)
    {
        characterMoved = true;
    }

    public void ClaimNameButtonAction()
    {
        Application.OpenURL("http://avatars.decentraland.org");

        ContinueAsGuestButtonAction();
    }

    public void ContinueAsGuestButtonAction()
    {
        claimNamePanel.SetActive(false);
        claimNamePanelClosed = true;
    }
}
