using DCL.TransactionHUDModel;

public interface ITransactionHUDController
{
    void Initialize();
    void ShowTransaction(ITransactionHUD transaction);
    void ShowTransaction(Model model);
    void SetActive(bool active);
    void Dispose();
}