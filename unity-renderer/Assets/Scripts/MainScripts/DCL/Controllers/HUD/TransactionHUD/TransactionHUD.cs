using System;
using TMPro;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;
using DCL.TransactionHUDModel;
using UnityEngine.SocialPlatforms.Impl;
using Type = DCL.TransactionHUDModel.Type;

public class TransactionHUD : MonoBehaviour, ITransactionHUD
{
    [SerializeField] private GameObject paymentPanel;
    
    [SerializeField] private GameObject signPanel;

    [SerializeField] private List<Button> acceptButtons;
    
    [SerializeField] private List<Button> rejectButtons;
    
    [SerializeField] private TMP_Text amountLabel;
    
    [SerializeField] private TMP_Text fromAddressLabel;

    [SerializeField] private TMP_Text toAddressLabel;
    
    [SerializeField] private TMP_Text messageLabel;
    
    [SerializeField] private TMP_Text paymentPanelTitle;
    
    [SerializeField] private TMP_Text signPanelTitle;
    
    [SerializeField] private TMP_Text paymentNetworkLabel;
    
    [SerializeField] private TMP_Text signNetworkLabel;

    public Model model { get; private set; } = new Model();

    public event Action<ITransactionHUD> OnTransactionAccepted;
    
    public event Action<ITransactionHUD> OnTransactionRejected;

    private void OnEnable()
    {
        foreach (var acceptButton in acceptButtons)
            acceptButton.onClick.AddListener(AcceptTransaction);
        
        foreach (var rejectButton in rejectButtons)
            rejectButton.onClick.AddListener(RejectTransaction);
    }
    
    public IParcelScene FindScene(string sceneId)
    {
        if (DCL.Environment.i.world?.state?.scenesSortedByDistance != null)
        {
            foreach (IParcelScene scene in DCL.Environment.i.world.state.scenesSortedByDistance)
            {
                if (scene.sceneData.id == sceneId)
                    return scene;
            }
        }

        return null;
    }

    private void OnDisable()
    {
        foreach (var acceptButton in acceptButtons)
            acceptButton.onClick.RemoveAllListeners();
        
        foreach (var rejectButton in rejectButtons)
            rejectButton.onClick.RemoveAllListeners();
    }

    private static string ShortAddress(string address)
    {
        if (address == null)
            return "Null";

        if (address.Length >= 12)
            return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
        
        return address;
    }
    
    private void ShowTransferMessage(Model model)
    {
        var scene = FindScene(model.sceneId);
        
        if (scene != null)
            paymentPanelTitle.text = $"'{scene.GetSceneName()}', {scene.sceneData.basePosition.ToString()} wants to initiate a transfer";
        paymentPanel.SetActive(true);
        signPanel.SetActive(false);

        UserProfile ownUserProfile = UserProfile.GetOwnUserProfile();
        fromAddressLabel.text = ShortAddress(ownUserProfile.ethAddress);
        toAddressLabel.text = ShortAddress(model.toAddress);
        amountLabel.text = $"{model.amount} {model.currency}";
        paymentNetworkLabel.text = KernelConfig.i.Get().network.ToUpper();
    }

    private void ShowSignMessage(Model model)
    {
        var scene = FindScene(model.sceneId);
        
        if (scene != null)
            signPanelTitle.text = $"'{scene.GetSceneName()}', {scene.sceneData.basePosition.ToString()} wants to sign this message";
        paymentPanel.SetActive(false);
        signPanel.SetActive(true);
        signNetworkLabel.text = KernelConfig.i.Get().network.ToUpper();
        messageLabel.text = model.message;
    }

    public void Show(Model model)
    {
        if (Utils.IsCursorLocked)
            Utils.UnlockCursor();

        this.model = model;

        if (model.requestType == Type.REQUIRE_PAYMENT)
            ShowTransferMessage(model);
        else
            ShowSignMessage(model);
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