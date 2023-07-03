using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Helpers;
using DCL.Interface;
using DCL.MyAccount;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;
using WaitUntil = UnityEngine.WaitUntil;

public class ProfileHUDController : IHUD
{
    [Serializable]
    public struct Configuration
    {
        public bool connectedWallet;
    }

    private const string URL_CLAIM_NAME = "https://builder.decentraland.org/claim-name";
    private const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
    private const string URL_MANA_PURCHASE = "https://account.decentraland.org";
    private const string URL_TERMS_OF_USE = "https://decentraland.org/terms";
    private const string URL_PRIVACY_POLICY = "https://decentraland.org/privacy";
    private const string LINKS_REGEX = @"\[(.*?)\)";
    private const float FETCH_MANA_INTERVAL = 60;

    public readonly IProfileHUDView view;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly DataStore dataStore;
    private readonly MyAccountCardController myAccountCardController;
    private readonly IBrowserBridge browserBridge;

    public event Action OnOpen;
    public event Action OnClose;

    internal AvatarEditorHUDController avatarEditorHud;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private Coroutine fetchManaIntervalRoutine = null;
    private Coroutine fetchPolygonManaIntervalRoutine = null;
    private CancellationTokenSource saveNameCancellationToken;
    private CancellationTokenSource saveDescriptionCancellationToken;

    private Regex nameRegex = null;

    public RectTransform TutorialTooltipReference => view.TutorialReference;

    public ProfileHUDController(
        IProfileHUDView view,
        IUserProfileBridge userProfileBridge,
        ISocialAnalytics socialAnalytics,
        DataStore dataStore,
        MyAccountCardController myAccountCardController,
        IBrowserBridge browserBridge)
    {
        this.userProfileBridge = userProfileBridge;
        this.socialAnalytics = socialAnalytics;
        this.dataStore = dataStore;
        this.view = view;
        this.myAccountCardController = myAccountCardController;
        this.browserBridge = browserBridge;

        dataStore.exploreV2.isOpen.OnChange += SetAsFullScreenMenuMode;
        dataStore.exploreV2.profileCardIsOpen.OnChange += SetProfileCardExtended;

        view.SetWalletSectionEnabled(false);
        view.SetNonWalletSectionEnabled(false);
        view.SetDescriptionIsEditing(false);

        view.LogedOutPressed += OnLoggedOut;
        view.SignedUpPressed += OnSignedUp;

        view.ClaimNamePressed += (object sender, EventArgs args) => browserBridge.OpenUrl(URL_CLAIM_NAME);

        view.Opened += (object sender, EventArgs args) =>
        {
            userProfileBridge.RequestOwnProfileUpdate();
            OnOpen?.Invoke();
        };

        view.Closed += (object sender, EventArgs args) => OnClose?.Invoke();
        view.NameSubmitted += (object sender, string name) => UpdateProfileName(name);
        view.DescriptionSubmitted += (object sender, string description) => UpdateProfileDescription(description);

        view.TermsAndServicesPressed += (object sender, EventArgs args) => browserBridge.OpenUrl(URL_TERMS_OF_USE);
        view.PrivacyPolicyPressed += (object sender, EventArgs args) => browserBridge.OpenUrl(URL_PRIVACY_POLICY);

        if (view.HasManaCounterView() || view.HasPolygonManaCounterView())
        {
            view.ManaInfoPressed += (object sender, EventArgs args) => browserBridge.OpenUrl(URL_MANA_INFO);
            view.ManaPurchasePressed += (object sender, EventArgs args) => browserBridge.OpenUrl(URL_MANA_PURCHASE);
        }

        ownUserProfile.OnUpdate += OnProfileUpdated;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        dataStore.exploreV2.isInitialized.OnChange += ExploreV2Changed;
        ExploreV2Changed(dataStore.exploreV2.isInitialized.Get(), false);
    }

    private void OnSignedUp(object sender, EventArgs e)
    {
        DCL.SettingsCommon.Settings.i.SaveSettings();
        userProfileBridge.SignUp();
    }

    private void OnLoggedOut(object sender, EventArgs e)
    {
        DCL.SettingsCommon.Settings.i.SaveSettings();
        userProfileBridge.LogOut();
    }

    private void SetProfileCardExtended(bool isOpenCurrent, bool previous)
    {
        OnOpen?.Invoke();
        view.ShowExpanded(isOpenCurrent, dataStore.myAccount.isInitialized.Get());
    }

    public void SetManaBalance(string balance) =>
        view?.SetManaBalance(balance);

    public void SetPolygonManaBalance(double balance) =>
        view?.SetPolygonBalance(balance);

    public void SetVisibility(bool visible)
    {
        if (visible && fetchManaIntervalRoutine == null)
            fetchManaIntervalRoutine = CoroutineStarter.Start(ManaIntervalRoutine());
        else if (!visible && fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (visible && fetchPolygonManaIntervalRoutine == null)
            fetchPolygonManaIntervalRoutine = CoroutineStarter.Start(PolygonManaIntervalRoutine());
        else if (!visible && fetchPolygonManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchPolygonManaIntervalRoutine);
            fetchPolygonManaIntervalRoutine = null;
        }
    }

    public virtual void Dispose()
    {
        view.LogedOutPressed -= OnLoggedOut;
        view.SignedUpPressed -= OnSignedUp;

        if (fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (view.GameObject)
            UnityEngine.Object.Destroy(view.GameObject);

        ownUserProfile.OnUpdate -= OnProfileUpdated;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

        dataStore.exploreV2.profileCardIsOpen.OnChange -= SetAsFullScreenMenuMode;
        dataStore.exploreV2.isInitialized.OnChange -= ExploreV2Changed;

        myAccountCardController.Dispose();
        saveNameCancellationToken.SafeCancelAndDispose();
        saveDescriptionCancellationToken.SafeCancelAndDispose();
    }

    private void OnProfileUpdated(UserProfile profile) =>
        view?.SetProfile(profile);

    private void ExploreV2Changed(bool current, bool previous) =>
        view.SetStartMenuButtonActive(current);

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) =>
        nameRegex = new Regex(current.profiles.nameValidRegex);

    private void UpdateProfileName(string newName)
    {
        if (nameRegex != null && !nameRegex.IsMatch(newName))
            return;

        saveNameCancellationToken = saveNameCancellationToken.SafeRestart();
        userProfileBridge.SaveUnverifiedName(newName, saveNameCancellationToken.Token).Forget();
    }

    private void UpdateProfileDescription(string description)
    {
        if (!ownUserProfile.hasConnectedWeb3 || view.IsDesciptionIsLongerThanMaxCharacters())
            return;

        saveDescriptionCancellationToken = saveDescriptionCancellationToken.SafeRestart();
        userProfileBridge.SaveDescription(description, saveDescriptionCancellationToken.Token).Forget();
        socialAnalytics.SendProfileEdit(description.Length, ContainsLinks(description), PlayerActionSource.ProfileEditHUD);
    }

    private void SetAsFullScreenMenuMode(bool currentIsFullScreenMenuMode, bool previousIsFullScreenMenuMode)
    {
        view.ShowProfileIcon(!currentIsFullScreenMenuMode);
    }

    private IEnumerator ManaIntervalRoutine()
    {
        while (true)
        {
            WebInterface.FetchBalanceOfMANA();
            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }

    private static bool ContainsLinks(string description) =>
        Regex.Matches(description, LINKS_REGEX, RegexOptions.IgnoreCase).Count > 0;

    private IEnumerator PolygonManaIntervalRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => ownUserProfile != null && !string.IsNullOrEmpty(ownUserProfile.userId));

            Promise<double> promise = Environment.i.platform.serviceProviders.theGraph.QueryMana(ownUserProfile.userId, TheGraphNetwork.Polygon);

            // This can be null if theGraph is mocked
            if (promise != null)
            {
                yield return promise;
                SetPolygonManaBalance(promise.value);
            }

            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }
}
