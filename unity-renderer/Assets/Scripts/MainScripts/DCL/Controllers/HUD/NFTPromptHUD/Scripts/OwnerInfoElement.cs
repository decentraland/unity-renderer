using System;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

internal interface IOwnerInfoElement : IDisposable
{
    void SetOwner(string address);
    void SetAddressLength(int length);
    void SetParent(Transform parent);
    void SetActive(bool active);
}

internal class OwnerInfoElement : MonoBehaviour, IOwnerInfoElement
{
    private const int DEFAULT_ADDRESS_MAX_CHARS = 11;

    [SerializeField] private TextMeshProUGUI textOwner;
    [SerializeField] private Image imageEllipse;

    private bool isDestroyed = false;
    private string address;
    private int lastAddressLength = DEFAULT_ADDRESS_MAX_CHARS;

    void IOwnerInfoElement.SetOwner(string ownerAddress)
    {
        address = ownerAddress;
        textOwner.text = NFTPromptHUDController.FormatOwnerAddress(ownerAddress, lastAddressLength);
        imageEllipse.color = Utils.GetColorForEthAddress(ownerAddress);
    }

    void IOwnerInfoElement.SetAddressLength(int length)
    {
        lastAddressLength = length;
        if (textOwner.text.Length != length)
        {
            textOwner.text = NFTPromptHUDController.FormatOwnerAddress(address, lastAddressLength);
        }
    }

    void IOwnerInfoElement.SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    void IOwnerInfoElement.SetActive(bool active) { gameObject.SetActive(active); }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() { isDestroyed = true; }
}