using Cysharp.Threading.Tasks;
using DCLServices.StableDiffusionService;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class AvatarEditorAIVariant : MonoBehaviour
{
    [SerializeField] private GameObject spinner;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private Button regenerateButton;
    [SerializeField] private Button saveToDiskButton;

    private Func<UniTask<Texture2D>> bodySnapshotRetriever;

    private void Awake()
    {
        avatarImage.gameObject.SetActive(false);
        spinner.gameObject.SetActive(false);
        regenerateButton.onClick.AddListener(OnRegenerateButtonPressed);
        saveToDiskButton.onClick.AddListener(OnSaveToDiskButtonPressed);
    }

    public void Initialize(Func<UniTask<Texture2D>> bodySnapshotRetriever)
    {
        this.bodySnapshotRetriever = bodySnapshotRetriever;
    }

    private void OnSaveToDiskButtonPressed()
    {

    }

    private void OnRegenerateButtonPressed()
    {
        GetAIVariant().Forget();
    }

    private async UniTask GetAIVariant()
    {
        regenerateButton.enabled = false;
        avatarImage.gameObject.SetActive(false);
        spinner.gameObject.SetActive(true);

        Texture2D bodysnapshot = await bodySnapshotRetriever.Invoke();
        var texture = await Environment.i.serviceLocator.Get<IStableDiffusionService>().GetTexture(bodysnapshot, new ImageToImageConfig()
        {
            cfgScale = 4,
            height = 512,
            width = 512,
            samplingSteps = 20,
            negativePrompt = "",
            prompt = "An avatar of decentraland, beautiful",
            denoisingStrength = 5,
            seed = -1,
        });

        avatarImage.texture = texture;

        avatarImage.gameObject.SetActive(true);
        spinner.gameObject.SetActive(false);
        regenerateButton.enabled = true;
    }
}
