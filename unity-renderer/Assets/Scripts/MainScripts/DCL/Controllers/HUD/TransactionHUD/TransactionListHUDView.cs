using System;
using DCL.TransactionHUDModel;
using UnityEngine;

public class TransactionListHUDView : MonoBehaviour
{
    [SerializeField]
    private RectTransform transactionPanel;

    public event Action<ITransactionHUD> OnTransactionAcceptedEvent;
    public event Action<ITransactionHUD> OnTransactionRejectedEvent;

    private const string VIEW_PATH = "TransactionListHUD";
    private const string VIEW_CHILD_PATH = "TransactionHUD";
    private const string VIEW_OBJECT_NAME = "_TransactionListHUD";

    internal static TransactionListHUDView Create()
    {
        TransactionListHUDView view = Instantiate(Resources.Load<TransactionListHUDView>(VIEW_PATH));
        view.Initialize();
        return view;
    }

    private void Initialize() { gameObject.name = VIEW_OBJECT_NAME; }

    public void ShowTransaction(ITransactionHUD transaction, Model model = null)
    {
        transaction.OnTransactionAccepted += OnTransactionAccepted;
        transaction.OnTransactionRejected += OnTransactionRejected;

        if (model != null)
            transaction.Show(model);
    }

    public TransactionHUD ShowTransaction(Model transactionModel)
    {
        if (transactionModel == null)
            return null;
        
        TransactionHUD transactionHUD = Instantiate(Resources.Load<TransactionHUD>(VIEW_CHILD_PATH), transactionPanel);
        ShowTransaction(transactionHUD, transactionModel);
        return transactionHUD;
    }

    private void OnTransactionAccepted(ITransactionHUD transaction)
    {
        OnTransactionAcceptedEvent?.Invoke(transaction);
    }

    private void OnTransactionRejected(ITransactionHUD transaction)
    {
        OnTransactionRejectedEvent?.Invoke(transaction);
    }
    
    public void SetActive(bool active) { gameObject.SetActive(active); }
}