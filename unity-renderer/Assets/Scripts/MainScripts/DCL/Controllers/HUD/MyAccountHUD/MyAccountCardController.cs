namespace DCL.MyAccount
{
    public class MyAccountCardController
    {
        private readonly IMyAccountCardComponentView view;
        private readonly DataStore dataStore;

        public MyAccountCardController(
            IMyAccountCardComponentView view,
            DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            view.OnAccountSettingsClicked += OnAccountSettingsClicked;
        }

        public void Dispose()
        {
            view.OnAccountSettingsClicked -= OnAccountSettingsClicked;
        }

        private void OnAccountSettingsClicked() =>
            dataStore.myAccount.myAccountSectionOpenFromProfileHUD.Set(true, true);
    }
}
