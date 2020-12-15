using DCL;
using UnityEngine;

public class NFTPromptHUDController : IHUD
{
    internal NFTPromptHUDView view { get; private set; }

    public NFTPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("NFTPromptHUD")).GetComponent<NFTPromptHUDView>();
        view.name = "_NFTPromptHUD";
        view.content.SetActive(false);

        if (Environment.i.sceneController != null)
            Environment.i.sceneController.OnOpenNFTDialogRequest += OpenNftInfoDialog;
    }

    public void OpenNftInfoDialog(string assetContractAddress, string tokenId, string comment)
    {
        if (!view.content.activeSelf)
        {
            view.ShowNFT(assetContractAddress, tokenId, comment);
        }
    }

    public void SetVisibility(bool visible)
    {
        view.content.SetActive(visible);

        AudioScriptableObjects.dialogOpen.Play(true);
    }

    public void Dispose()
    {
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }

        if (Environment.i != null)
            Environment.i.sceneController.OnOpenNFTDialogRequest -= OpenNftInfoDialog;
    }
}