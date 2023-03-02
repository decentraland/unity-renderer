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
    [SerializeField] private Button acceptButton;
    
    [SerializeField] private Button rejectButton;
    
    [SerializeField] private TMP_Text messageLabel;

    public Model model { get; private set; } = new Model();

    public event Action<ITransactionHUD> OnTransactionAccepted;
    
    public event Action<ITransactionHUD> OnTransactionRejected;

    private void OnEnable()
    {
        acceptButton.onClick.AddListener(AcceptTransaction);
        
        rejectButton.onClick.AddListener(RejectTransaction);
    }
    
    public IParcelScene FindScene(int sceneNumber)
    {
        DCL.Environment.i.world.state.TryGetScene(sceneNumber, out var scene);
        return scene;
    }

    private void OnDisable()
    {
        acceptButton.onClick.RemoveAllListeners();
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

    private void ShowSignMessage(Model model)
    {
        var scene = FindScene(model.sceneNumber);

        if (scene != null)
        {
            messageLabel.text = $"This scene {scene.sceneData.basePosition.ToString()} is requesting the signature of a message. " +
                                $"If you are interested, please click the Allow button to receive it in your Wallet Connect application. " +
                                $"This action does not imply message approval.";
        }
    }

    public void Show(Model model)
    {
        if (Utils.IsCursorLocked)
            Utils.UnlockCursor();

        this.model = model;
        
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