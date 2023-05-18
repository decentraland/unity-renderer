using Cysharp.Threading.Tasks;
using DCLServices.StableDiffusionService;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class AvatarEditorAIVariant : MonoBehaviour
{
    [SerializeField] private GameObject spinner;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private Button regenerateButton;
    [SerializeField] private Button saveToDiskButton;
    [SerializeField] private TMP_InputField mainStyleInputField;


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

        string mainStyle = mainStyleInputField.text ?? "furry";

        Texture2D bodysnapshot = await bodySnapshotRetriever.Invoke();
        var texture = await Environment.i.serviceLocator.Get<IStableDiffusionService>().GetTexture(bodysnapshot, new ImageToImageConfig()
        {
            cfgScale = 5,
            height = 512,
            width = 256,
            samplingSteps = 40,
            negativePrompt = "ugly, tiling, poorly drawn hands, poorly drawn feet, poorly drawn face, out of frame, extra limbs, disfigured, deformed, body out of frame, bad anatomy, watermark, signature, cut off, low contrast, underexposed, overexposed, bad art, beginner, amateur, distorted face, blurry, draft, grainy",
            //prompt = $"An avatar of decentraland, beautiful, detailed clothing, digital painting, hyperrealistic, photorealistic, fantasy,  artstation, highly detailed, sharp focus, sci-fi, stunningly beautiful, dystopian, iridescent gold, cinematic lighting, dark,  (((({mainStyle}))))",
            prompt = $"(({mainStyle})) character,  fantasy,  sharp focus, in the city, cyberpunk",
            denoisingStrength = 0.42f,
            seed = -1,
        });

        avatarImage.texture = texture;

        avatarImage.gameObject.SetActive(true);
        spinner.gameObject.SetActive(false);
        regenerateButton.enabled = true;
    }
}
