using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PassportPlayerInfoComponentView : MonoBehaviour, IPassportPlayerInfoComponentView
{
    [SerializeField] internal TextMeshProUGUI name;
    [SerializeField] internal Button walletCopyButton;
    [SerializeField] internal TextMeshProUGUI wallet;

    private string fullWalletAddress;

    private void Start() 
    {
        walletCopyButton.onClick.AddListener(CopyWalletToClipboard);
    }

    public void SetName(string name)
    {
        this.name.text = name;
    }

    public void SetWallet(string wallet)
    {
        fullWalletAddress = wallet;
        this.wallet.text = $"{wallet.Substring(0,5)}...{wallet.Substring(wallet.Length - 5)}";
    }

    internal void CopyWalletToClipboard()
    {
        if(fullWalletAddress == null)
            return;
        
        GUIUtility.systemCopyBuffer = fullWalletAddress;
    }
}
