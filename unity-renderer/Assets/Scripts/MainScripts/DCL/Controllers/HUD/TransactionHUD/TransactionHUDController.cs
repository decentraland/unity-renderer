using System.Collections.Generic;
using UnityEngine;

public class TransactionHUDController : IHUD, ITransactionHUDController
{
    [System.Serializable]
    public class Model
    {
        public List<ITransactionHUD> transactions = new List<ITransactionHUD>();
    }

    public TransactionListHUDView view { get; private set; }
    public Model model { get; private set; }
    public TransactionHUDController() : this(new Model()) { }

    public TransactionHUDController(Model model)
    {
        this.model = model;
        view = TransactionListHUDView.Create();
        view.OnTransactionAcceptedEvent += OnTransactionAccepted;
        view.OnTransactionRejectedEvent += OnTransactionRejected;
    }

    public void ShowTransaction(ITransactionHUD transaction)
    {
        model.transactions.Add(transaction);
        view.ShowTransaction(transaction, transaction.model);
    }
    public void ShowTransaction(DCL.TransactionHUDModel.Model model)
    {
        var transaction = view.ShowTransaction(model);
        this.model.transactions.Add(transaction);
    }

    private void OnTransactionAccepted(ITransactionHUD transaction)
    {
        DCL.Interface.WebInterface.Web3UseResponse(transaction.model.id, true);
        model.transactions.Remove(transaction);
    }
    private void OnTransactionRejected(ITransactionHUD transaction)
    {
        DCL.Interface.WebInterface.Web3UseResponse(transaction.model.id, false);
        model.transactions.Remove(transaction);
    }

    public void SetActive(bool active) { view.SetActive(active); }

    public void Dispose()
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }

        view.OnTransactionAcceptedEvent -= OnTransactionAccepted;
        view.OnTransactionRejectedEvent -= OnTransactionRejected;
    }

    public void SetVisibility(bool visible) { SetActive(visible); }
}