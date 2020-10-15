using DCL.Interface;

namespace DCL.GoToGenesisPlazaHUD
{
    public class GoToGenesisPlazaHUDController : IHUD
    {
        public GoToGenesisPlazaHUDView view { private set; get; }

        public event System.Action<bool> OnOpen;
        public event System.Action OnBeforeGoToGenesisPlaza;
        public event System.Action OnAfterGoToGenesisPlaza;

        public GoToGenesisPlazaHUDController()
        {
            view = GoToGenesisPlazaHUDView.Create();

            view.OnContinueClick += () => OnGoToGenesisButtonClick();
            view.OnCancelClick += () => SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
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

            SetVisibility(false);
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
    }
}