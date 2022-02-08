using System.Collections.Generic;
using DCL;
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

    private TransactionBridge transactionBridge;

    public TransactionHUDController(Model model)
    {
        this.model = model;
    }

    public void Initialize()
    {
        view = TransactionListHUDView.Create();
        view.OnTransactionAcceptedEvent += OnTransactionAccepted;
        view.OnTransactionRejectedEvent += OnTransactionRejected;

        transactionBridge = SceneReferences.i.bridgeGameObject.AddComponent<TransactionBridge>();
        transactionBridge.transactionController = this;
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
        if (transactionBridge != null)
        {
            Object.Destroy(transactionBridge);
        }
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }

        view.OnTransactionAcceptedEvent -= OnTransactionAccepted;
        view.OnTransactionRejectedEvent -= OnTransactionRejected;
    }

    public void SetVisibility(bool visible) { SetActive(visible); }
}