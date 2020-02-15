namespace DCL.Tutorial
{
    public class TutorialStep_GenesisPlaza : TutorialStep
    {
        public override void OnStepStart()
        {
            base.OnStepStart();
            HUDController.i?.minimapHud.SetVisibility(true);
            HUDController.i?.expressionsHud.SetVisibility(false);
            TutorialController.i.SetChatVisible(false);
        }
    }
}
