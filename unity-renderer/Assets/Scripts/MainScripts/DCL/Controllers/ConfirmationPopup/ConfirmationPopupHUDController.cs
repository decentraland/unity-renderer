using System;

namespace DCL.ConfirmationPopup
{
    public class ConfirmationPopupHUDController : IDisposable
    {
        private readonly IConfirmationPopupHUDView view;
        private readonly DataStore_Notifications dataStore;

        private Action externalConfirmationAction;
        private Action externalCancelAction;

        public ConfirmationPopupHUDController(IConfirmationPopupHUDView view,
            DataStore_Notifications dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            this.view.OnConfirm += OnConfirmFromView;
            this.view.OnCancel += OnCancelFromView;

            this.dataStore.GenericConfirmation.OnChange += ShowOrHide;

            view.Hide(true);
        }

        public void Dispose()
        {
            dataStore.GenericConfirmation.OnChange -= ShowOrHide;
            view.OnConfirm -= OnConfirmFromView;
            view.OnCancel -= OnCancelFromView;
            view.Dispose();
            externalCancelAction = null;
            externalConfirmationAction = null;
        }

        private void OnCancelFromView()
        {
            externalCancelAction?.Invoke();
            Hide();
        }

        private void OnConfirmFromView()
        {
            externalConfirmationAction?.Invoke();
            Hide();
        }

        private void ShowOrHide(GenericConfirmationNotificationData current, GenericConfirmationNotificationData previous)
        {
            if (current == null)
                Hide();
            else
                Show(current);
        }

        private void Hide()
        {
            externalCancelAction = null;
            externalConfirmationAction = null;
            view.Hide();
        }

        private void Show(GenericConfirmationNotificationData data)
        {
            externalCancelAction = data.CancelAction;
            externalConfirmationAction = data.ConfirmAction;
            view.SetModel(new ConfirmationPopupHUDViewModel(data.Title, data.Body, data.CancelButton, data.ConfirmButton));
            view.Show();
        }
    }
}
