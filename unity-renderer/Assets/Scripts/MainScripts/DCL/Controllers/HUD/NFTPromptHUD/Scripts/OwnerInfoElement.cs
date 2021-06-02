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
    void SetColorIndex(int index);
    void SetParent(Transform parent);
    void SetActive(bool active);
    void SetConfig(float fontSize, float horizontalSpacing);
}

internal class OwnerInfoElement : MonoBehaviour, IOwnerInfoElement
{
    private const int DEFAULT_ADDRESS_MAX_CHARS = 11;

    [SerializeField] private TextMeshProUGUI textOwner;
    [SerializeField] private Image imageEllipse;
    [SerializeField] private HorizontalLayoutGroup horizontalLayout;
    [SerializeField] private Color[] colorsEllipse;

    private bool isDestroyed = false;
    private string address;
    private int lastAddressLength = DEFAULT_ADDRESS_MAX_CHARS;

    void IOwnerInfoElement.SetOwner(string ownerAddress)
    {
        address = ownerAddress;
        textOwner.text = NFTPromptHUDController.FormatOwnerAddress(ownerAddress, lastAddressLength);
    }

    void IOwnerInfoElement.SetAddressLength(int length)
    {
        lastAddressLength = length;
        if (textOwner.text.Length != length)
        {
            textOwner.text = NFTPromptHUDController.FormatOwnerAddress(address, lastAddressLength);
        }
    }

    void IOwnerInfoElement.SetColorIndex(int index)
    {
        int colorIndex = index % colorsEllipse.Length;
        imageEllipse.color = colorsEllipse[colorIndex];
    }

    void IOwnerInfoElement.SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    void IOwnerInfoElement.SetConfig(float fontSize, float horizontalSpacing)
    {
        textOwner.fontSize = fontSize;
        horizontalLayout.spacing = horizontalSpacing;
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