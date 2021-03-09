public interface IConfirmationDialog
{
    void SetText(string text);
    void Show(System.Action onConfirm = null, System.Action onCancel = null);
    void Hide();
}
