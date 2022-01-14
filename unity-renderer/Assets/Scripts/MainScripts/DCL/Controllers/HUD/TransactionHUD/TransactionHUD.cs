using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.TransactionHUDModel;
using Type = DCL.TransactionHUDModel.Type;

public class TransactionHUD : MonoBehaviour, ITransactionHUD
{
    [SerializeField] private GameObject paymentPanel;
    
    [SerializeField] private GameObject signPanel;

    [SerializeField] private List<Button> acceptButtons;
    
    [SerializeField] private List<Button> rejectButtons;
    
    [SerializeField] private TMP_Text amountLabel;
    
    [SerializeField] private TMP_Text toAddressLabel;
    
    [SerializeField] private TMP_Text messageLabel;

    public Model model { get; private set; } = new Model();

    public event Action<ITransactionHUD> OnTransactionAccepted;
    
    public event Action<ITransactionHUD> OnTransactionRejected;

    private void OnEnable()
    {
        foreach (var acceptButton in acceptButtons)
            acceptButton.onClick.AddListener(AcceptTransaction);
        
        foreach (var rejectButton in rejectButtons)
            rejectButton.onClick.AddListener(RejectTransaction);

        //AudioScriptableObjects.notification.Play(true);
    }

    private void OnDisable()
    {
        foreach (var acceptButton in acceptButtons)
            acceptButton.onClick.RemoveAllListeners();
        
        foreach (var rejectButton in rejectButtons)
            rejectButton.onClick.RemoveAllListeners();
    }

    public void Show(Model model)
    {
        this.model = model;

        if (model.requestType == Type.REQUIRE_PAYMENT)
        {
            paymentPanel.SetActive(true);
            signPanel.SetActive(false);

            toAddressLabel.text = model.toAddress;
            amountLabel.text = $"{model.amount} {model.currency}";
        }
        else
        {
            paymentPanel.SetActive(false);
            signPanel.SetActive(true);

            messageLabel.text = model.message;
        }
    }

    public void AcceptTransaction()
    {
        OnTransactionAccepted?.Invoke(this);
        Destroy(gameObject);
    }

    public void RejectTransaction()
    {
        OnTransactionRejected?.Invoke(this);
        Destroy(gameObject);
    }
}