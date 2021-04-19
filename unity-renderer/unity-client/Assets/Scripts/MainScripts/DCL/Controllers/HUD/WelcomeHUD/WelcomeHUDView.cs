using System;
using System.Collections;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IWelcomeHUDView
{
    void Initialize(UnityAction<int> OnConfirm, UnityAction OnClose, MessageOfTheDayConfig config);
    void DisposeSelf();
    void SetVisible(bool visible);
}

public class WelcomeHUDView : MonoBehaviour, IWelcomeHUDView
{
    private const string PREFAB_PATH = "WelcomeHUD";

    [SerializeField] internal RawImage backgroundImage;
    [SerializeField] internal TextMeshProUGUI titleText;
    [SerializeField] internal TextMeshProUGUI timeLeftText;
    [SerializeField] internal TextMeshProUGUI bodyText;
    [SerializeField] internal Transform buttonsParent;
    [SerializeField] internal Button_OnPointerDown closeButton;

    [Space]
    [SerializeField] internal GameObject buttonPrefab;

    [Space]
    [SerializeField] internal bool useImageNativeSize = false;

    private AssetPromise_Texture texturePromise;
    private Coroutine updateTimeCoroutine;
    private UnityAction OnCloseButtonPressed;

    private bool disposed = false;

    public static WelcomeHUDView CreateView() => Instantiate(Resources.Load<GameObject>(PREFAB_PATH)).GetComponent<WelcomeHUDView>();

    public void Initialize(UnityAction<int> OnConfirm, UnityAction OnClose, MessageOfTheDayConfig config)
    {
        if (config == null)
        {
            CleanUp();
            return;
        }

        ClearButtons();
        titleText.text = config.title;
        bodyText.text = config.body;

        SetupButtons(config.buttons, OnConfirm);
        closeButton.onPointerDown -= OnCloseButtonPressed;
        OnCloseButtonPressed = () => OnClose?.Invoke();
        closeButton.onPointerDown += OnCloseButtonPressed;

        CleanUpPromise();
        if (!String.IsNullOrEmpty(config.background_banner))
        {
            texturePromise = new AssetPromise_Texture(config.background_banner);
            texturePromise.OnSuccessEvent += OnTextureRetrieved;
        }

        AssetPromiseKeeper_Texture.i.Keep(texturePromise);

        if (config.endUnixTimestamp > 0)
        {
            timeLeftText.gameObject.SetActive(false);
            if (updateTimeCoroutine == null)
                updateTimeCoroutine = StartCoroutine(UpdateTimer(DateTimeOffset.FromUnixTimeSeconds(config.endUnixTimestamp).LocalDateTime));
        }
        else
        {
            timeLeftText.gameObject.SetActive(false);
            if (updateTimeCoroutine != null)
                StopCoroutine(updateTimeCoroutine);
        }
    }

    private void OnTextureRetrieved(Asset_Texture assetTexture)
    {
        backgroundImage.texture = assetTexture.texture;

        if(useImageNativeSize)
            backgroundImage.SetNativeSize();
    }

    private IEnumerator UpdateTimer(DateTime localEndDate)
    {
        while (true)
        {
            TimeSpan remainingTime = DateTime.Now > localEndDate ? TimeSpan.Zero : (localEndDate - DateTime.Now);
            timeLeftText.text = $"Ending in: {remainingTime.Days:00}:{remainingTime.Hours:00}:{remainingTime.Minutes:00}:{remainingTime.Seconds:00}";
            yield return new WaitForSeconds(.5f);
        }
    }

    private void SetupButtons(MessageOfTheDayConfig.Button[] buttons, UnityAction<int> buttonsCallback)
    {
        if (buttons == null)
            return;

        for (var i = 0; i < buttons.Length; i++)
        {
            int index = i;
            GameObject buttonGO = Instantiate(buttonPrefab, buttonsParent);

            Button_OnPointerDown button = buttonGO.GetComponentInChildren<Button_OnPointerDown>();
            if(button != null)
                button.onPointerDown += () => buttonsCallback?.Invoke(index);

            TextMeshProUGUI caption = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if(caption != null)
                caption.text = buttons[i].caption;

            Image buttonImage = buttonGO.GetComponentInChildren<Image>();
            if (buttonImage != null)
                buttonImage.color = buttons[i].tint;
        }
    }

    private void ClearButtons()
    {
        for(int i = buttonsParent.childCount -1; i >= 0; i--)
        {
            Destroy(buttonsParent.GetChild(i).gameObject);
        }
    }

    private void CleanUp()
    {
        CleanUpPromise();
        ClearButtons();
        closeButton.onPointerDown -= OnCloseButtonPressed;
    }

    private void CleanUpPromise()
    {
        if (texturePromise == null)
            return;

        AssetPromiseKeeper_Texture.i.Forget(texturePromise);
        texturePromise.Cleanup();
        texturePromise = null;
    }

    public void DisposeSelf()
    {
        if (!disposed)
        {
            disposed = true;
            Destroy(gameObject);
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        disposed = true;
        CleanUp();
    }
}
