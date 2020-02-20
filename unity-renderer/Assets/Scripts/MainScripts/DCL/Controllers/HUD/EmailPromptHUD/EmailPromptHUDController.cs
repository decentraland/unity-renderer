using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCL.Interface;

public class EmailPromptHUDController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject cryptoUserMessage;
    public GameObject nonCryptoUserMessage;
    public Button sendButton;

    void Awake()
    {
        bool hasWallet = UserProfile.GetOwnUserProfile().hasConnectedWeb3;

        cryptoUserMessage.SetActive(hasWallet);
        nonCryptoUserMessage.SetActive(!hasWallet);

        sendButton.interactable = false;
        sendButton.onClick.AddListener(SaveEmail);

        inputField.onValueChanged.AddListener(value =>
        {
            sendButton.interactable = !string.IsNullOrEmpty(value);
        });
    }

    public void SaveEmail()
    {
        gameObject.SetActive(false);
        WebInterface.SendUserEmail(inputField.text);
    }
}
