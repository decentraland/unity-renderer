using System;
using DCL.TransactionHUDModel;

public interface ITransactionHUD
{
    event Action<ITransactionHUD> OnTransactionAccepted;
    event Action<ITransactionHUD> OnTransactionRejected;
    Model model { get; }
    void Show(Model model);
    void AcceptTransaction();
    void RejectTransaction();
}