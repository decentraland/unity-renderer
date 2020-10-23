using DCL.Interface;

namespace DCL.GoToGenesisPlazaHUD
{
    public class GoToGenesisPlazaHUDController : IHUD
    {
        public GoToGenesisPlazaHUDView view { private set; get; }

        public event System.Action<bool> OnOpen;
        public event System.Action OnBeforeGoToGenesisPlaza;
        public event System.Action OnAfterGoToGenesisPlaza;

        private bool teleportToGenesisPlaza = false;

        public GoToGenesisPlazaHUDController()
        {
            view = GoToGenesisPlazaHUDView.Create();

            view.OnContinueClick += () => OnGoToGenesisButtonClick();
            view.OnCancelClick += () => SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (visible && !view.isOpen)
            {
                AudioScriptableObjects.dialogOpen.Play(true);
                SendGoToGenesisPlazaHUDInteractionSegmentStats(true, false);
            }
            else if (!visible && view.isOpen)
            {
                AudioScriptableObjects.dialogClose.Play(true);

                if (!teleportToGenesisPlaza)
                    SendGoToGenesisPlazaHUDInteractionSegmentStats(false, false);
            }

            OnOpen?.Invoke(visible);
            view.SetVisibility(visible);
        }

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }

        private void OnGoToGenesisButtonClick()
        {
            CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;

            teleportToGenesisPlaza = true;
            SetVisibility(false);
            SendGoToGenesisPlazaHUDInteractionSegmentStats(false, true);
            WebInterface.GoTo(0, 0);
            OnBeforeGoToGenesisPlaza?.Invoke();
        }

        private void RendererState_OnChange(bool current, bool previous)
        {
            if (current)
            {
                CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
                if (SceneController.i != null)
                    SceneController.i.OnSortScenes += SceneController_OnSortScenes;
            }
        }

        private void SceneController_OnSortScenes()
        {
            SceneController.i.OnSortScenes -= SceneController_OnSortScenes;

            OnAfterGoToGenesisPlaza?.Invoke();
        }

        private void SendGoToGenesisPlazaHUDInteractionSegmentStats(bool openPopUp, bool jumpInGenesisPlaza)
        {
            string interactionType = string.Empty;
            if (jumpInGenesisPlaza)
                interactionType = "jump in genesis plaza";
            else
                interactionType = openPopUp ? "open" : "closed";

            WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]
            {
                new WebInterface.AnalyticsPayload.Property("interaction", interactionType)
            };
            WebInterface.ReportAnalyticsEvent("genesis plaza popup interaction", properties);
        }
    }
}
