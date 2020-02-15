using UnityEngine;
using TMPro;

public class EmailPromptHUDController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject cryptoUserMessage;
    public GameObject nonCryptoUserMessage;

    void Awake()
    {
        bool hasWallet = UserProfile.GetOwnUserProfile().hasConnectedWeb3;

        cryptoUserMessage.SetActive(hasWallet);
        nonCryptoUserMessage.SetActive(!hasWallet);
    }

    public void SaveEmail()
    {
        // TODO AFTER HAVING THE REWARD ON TUTORIAL
    }
}
