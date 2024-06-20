using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.DCLFileBrowser;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Components.Avatar.VRMExporter;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController
    {
        private const string NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF = "new_terms_of_service_and_email_subscription";
        private const string EMOTE_ID = "wave";
        private const string NOT_LOADED = "NOT_LOADED";

        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly IBackpackEmotesSectionController backpackEmotesSectionController;
        private readonly IBackpackAnalyticsService backpackAnalyticsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly RendererState rendererState;
        private readonly WearableGridController wearableGridController;
        private readonly AvatarSlotsHUDController avatarSlotsHUDController;
        private readonly OutfitsController outfitsController;
        private readonly IVRMExporter vrmExporter;
        private readonly VRMDetailsController vrmDetailsController;
        private readonly IDCLFileBrowserService fileBrowser;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly Dictionary<string, string> extendedWearableUrns = new ();
        private readonly Dictionary<string, Dictionary<string, string>> fallbackWearables = new ()
        {
            {"urn:decentraland:off-chain:base-avatars:BaseFemale", new Dictionary<string, string>
            {
                {WearableLiterals.Categories.UPPER_BODY, "urn:decentraland:off-chain:base-avatars:white_top"},
                {WearableLiterals.Categories.LOWER_BODY, "urn:decentraland:off-chain:base-avatars:f_jeans"},
                {WearableLiterals.Categories.FEET, "urn:decentraland:off-chain:base-avatars:ruby_blue_loafer"},
                {WearableLiterals.Categories.HAIR, "urn:decentraland:off-chain:base-avatars:pony_tail"},
                {WearableLiterals.Categories.MOUTH, "urn:decentraland:off-chain:base-avatars:f_mouth_05"},
                {WearableLiterals.Categories.EYEBROWS, "urn:decentraland:off-chain:base-avatars:f_eyebrows_02"},
                {WearableLiterals.Categories.EYES, "urn:decentraland:off-chain:base-avatars:f_eyes_06"},
            }},
            {"urn:decentraland:off-chain:base-avatars:BaseMale", new Dictionary<string, string>
            {
                {WearableLiterals.Categories.UPPER_BODY, "urn:decentraland:off-chain:base-avatars:m_sweater_02"},
                {WearableLiterals.Categories.LOWER_BODY, "urn:decentraland:off-chain:base-avatars:soccer_pants"},
                {WearableLiterals.Categories.FEET, "urn:decentraland:off-chain:base-avatars:sport_colored_shoes"},
                {WearableLiterals.Categories.HAIR, "urn:decentraland:off-chain:base-avatars:cool_hair"},
                {WearableLiterals.Categories.FACIAL_HAIR, "urn:decentraland:off-chain:base-avatars:beard"},
                {WearableLiterals.Categories.EYEBROWS, "urn:decentraland:off-chain:base-avatars:eyebrows_00"},
                {WearableLiterals.Categories.EYES, "urn:decentraland:off-chain:base-avatars:eyes_00"},
            }}
        };
        private readonly BackpackEditorHUDModel model = new ();
        private readonly Dictionary<string, bool> vrmBlockingWearables = new (); // Key: id, Value: canBeUnEquipped

        private string currentSlotSelected;
        private bool avatarIsDirty;
        private CancellationTokenSource loadProfileCancellationToken = new ();
        private CancellationTokenSource setVisibilityCancellationToken = new ();
        private CancellationTokenSource outfitLoadCancellationToken = new ();
        private CancellationTokenSource saveCancellationToken = new ();
        private string categoryPendingToPlayEmote;
        private bool isTakingSnapshot;

        private bool isNewTermsOfServiceAndEmailSubscriptionEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled(NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF);

        private BaseCollection<string> previewEquippedWearables => dataStore.backpackV2.previewEquippedWearables;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private int currentAnimationIndexShown;
        private bool shouldRequestOutfits = true;

        private CancellationTokenSource vrmExportCts;

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            RendererState rendererState,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            IBackpackEmotesSectionController backpackEmotesSectionController,
            IBackpackAnalyticsService backpackAnalyticsService,
            WearableGridController wearableGridController,
            AvatarSlotsHUDController avatarSlotsHUDController,
            OutfitsController outfitsController,
            IVRMExporter vrmExporter,
            VRMDetailsController vrmDetailsController,
            IDCLFileBrowserService fileBrowser,
            IEmotesCatalogService emotesCatalogService)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.rendererState = rendererState;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.backpackEmotesSectionController = backpackEmotesSectionController;
            this.backpackAnalyticsService = backpackAnalyticsService;
            this.wearableGridController = wearableGridController;
            this.avatarSlotsHUDController = avatarSlotsHUDController;
            this.outfitsController = outfitsController;
            this.vrmExporter = vrmExporter;
            this.vrmDetailsController = vrmDetailsController;
            this.fileBrowser = fileBrowser;
            this.emotesCatalogService = emotesCatalogService;

            avatarSlotsHUDController.GenerateSlots();
            ownUserProfile.OnUpdate += LoadUserProfileFromProfileUpdate;
            dataStore.HUDs.avatarEditorVisible.OnChange += OnBackpackVisibleChanged;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;
            dataStore.backpackV2.isWaitingToBeSavedAfterSignUp.OnChange += SaveBackpackBeforeSignUpFinishes;

            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);

            backpackEmotesSectionController.OnNewEmoteAdded += OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed += OnEmotePreviewed;
            backpackEmotesSectionController.OnEmoteEquipped += OnEmoteEquipped;
            backpackEmotesSectionController.OnEmoteUnEquipped += OnEmoteUnEquipped;

            wearableGridController.OnWearableSelected += SelectWearable;
            wearableGridController.OnWearableEquipped += EquipWearableFromGrid;
            wearableGridController.OnWearableUnequipped += UnEquipWearableFromGrid;
            wearableGridController.OnCategoryFilterRemoved += OnCategoryFilterRemoved;

            avatarSlotsHUDController.OnToggleSlot += ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot += UnEquipWearableFromSlot;
            avatarSlotsHUDController.OnHideUnhidePressed += UpdateOverrideHides;

            view.SetColorPickerVisibility(false);
            view.OnContinueSignup += SaveAvatarAndContinueSignupProcess;
            view.OnColorChanged += OnWearableColorChanged;
            view.OnColorPickerToggle += OnColorPickerToggled;
            view.OnAvatarUpdated += OnAvatarUpdated;
            view.OnOutfitsOpened += OnOutfitsOpened;
            view.OnSignUpBackClicked += OnSignUpBack;
            outfitsController.OnOutfitEquipped += OnOutfitEquipped;

            view.SetOutfitsEnabled(dataStore.featureFlags.flags.Get().IsFeatureEnabled("outfits"));
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get(), saveAvatar: false);

            bool vrmFeatureEnabled = this.dataStore.featureFlags.flags.Get().IsFeatureEnabled("vrm_export");
            view.OnVRMExport += OnVrmExport;
            view.OnVRMDetailsOpened += OnVRMDetailsOpened;
            view.OnVRMDetailsClosed += UpdateVRMExportWarning;
            view.SetVRMButtonActive(vrmFeatureEnabled);
            view.SetVRMButtonEnabled(vrmFeatureEnabled);
            view.SetVRMSuccessToastActive(false);

            vrmDetailsController.OnWearableUnequipped += UnEquipWearableFromGrid;
            vrmDetailsController.OnWearableEquipped += EquipWearableFromGrid;
            UpdateVRMExportWarning();
        }

        private void OnVRMDetailsOpened()
        {
            vrmDetailsController.Initialize(vrmBlockingWearables);
        }

        private void OnOutfitEquipped(OutfitItem outfit)
        {
            Dictionary<string, WearableItem> keyValuePairs = new Dictionary<string, WearableItem>(model.wearables);

            foreach (KeyValuePair<string, WearableItem> keyValuePair in keyValuePairs)
                UnEquipWearable(keyValuePair.Key, UnequipWearableSource.None, false, false);

            foreach (string forcedCategory in model.forceRender)
                UpdateOverrideHides(forcedCategory, false, false);

            outfitLoadCancellationToken = new CancellationTokenSource();
            LoadAndEquipOutfitWearables(outfit, outfitLoadCancellationToken.Token).Forget();
        }

        private async UniTaskVoid LoadAndEquipOutfitWearables(OutfitItem outfit, CancellationToken cancellationToken)
        {
            if (!wearablesCatalogService.WearablesCatalog.ContainsKey(outfit.outfit.bodyShape))
                await wearablesCatalogService.RequestWearableAsync(outfit.outfit.bodyShape, cancellationToken);

            foreach (string outfitWearable in outfit.outfit.wearables)
            {
                if (wearablesCatalogService.WearablesCatalog.ContainsKey(ExtendedUrnParser.GetShortenedUrn(outfitWearable))) continue;

                try { await wearablesCatalogService.RequestWearableAsync(outfitWearable, cancellationToken); }
                catch (Exception e) { Debug.LogWarning($"Cannot resolve the wearable {outfitWearable} for the outfit {outfit.slot.ToString()}"); }
            }

            EquipWearable(outfit.outfit.bodyShape, EquipWearableSource.Outfit, setAsDirty: false, updateAvatarPreview: false);

            foreach (string outfitWearable in outfit.outfit.wearables)
                EquipWearable(outfitWearable, EquipWearableSource.Outfit, setAsDirty: true, updateAvatarPreview: true);

            SetAllColors(outfit.outfit.eyes.color, outfit.outfit.hair.color, outfit.outfit.skin.color);

            foreach (string forcedCategory in outfit.outfit.forceRender)
                UpdateOverrideHides(forcedCategory, true, true);
        }

        private void OnOutfitsOpened()
        {
            if (!shouldRequestOutfits) return;

            outfitsController.RequestOwnedOutfits();
            shouldRequestOutfits = false;
        }

        public void Dispose()
        {
            vrmExportCts?.SafeCancelAndDispose();
            ownUserProfile.OnUpdate -= LoadUserProfileFromProfileUpdate;
            dataStore.HUDs.avatarEditorVisible.OnChange -= OnBackpackVisibleChanged;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            dataStore.backpackV2.isWaitingToBeSavedAfterSignUp.OnChange -= SaveBackpackBeforeSignUpFinishes;

            backpackEmotesSectionController.OnNewEmoteAdded -= OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed -= OnEmotePreviewed;
            backpackEmotesSectionController.OnEmoteEquipped -= OnEmoteEquipped;
            backpackEmotesSectionController.OnEmoteUnEquipped -= OnEmoteUnEquipped;
            backpackEmotesSectionController.Dispose();

            wearableGridController.OnWearableSelected -= SelectWearable;
            wearableGridController.OnWearableEquipped -= EquipWearableFromGrid;
            wearableGridController.OnWearableUnequipped -= UnEquipWearableFromGrid;
            wearableGridController.OnCategoryFilterRemoved -= OnCategoryFilterRemoved;
            wearableGridController.Dispose();

            avatarSlotsHUDController.OnToggleSlot -= ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot -= UnEquipWearableFromSlot;
            avatarSlotsHUDController.OnHideUnhidePressed -= UpdateOverrideHides;
            avatarSlotsHUDController.Dispose();

            view.OnColorChanged -= OnWearableColorChanged;
            view.OnContinueSignup -= SaveAvatarAndContinueSignupProcess;
            view.OnColorPickerToggle -= OnColorPickerToggled;
            view.OnAvatarUpdated -= OnAvatarUpdated;
            view.OnOutfitsOpened -= OnOutfitsOpened;
            view.OnVRMExport -= OnVrmExport;
            view.OnSignUpBackClicked -= OnSignUpBack;
            outfitsController.OnOutfitEquipped -= OnOutfitEquipped;
            view.Dispose();

            vrmBlockingWearables.Clear();
            view.OnVRMDetailsOpened -= OnVRMDetailsOpened;
            view.OnVRMDetailsClosed -= UpdateVRMExportWarning;
            vrmDetailsController.OnWearableUnequipped -= UnEquipWearableFromGrid;
            vrmDetailsController.OnWearableEquipped -= EquipWearableFromGrid;
        }

        private void UpdateOverrideHides(string category, bool toggleOn) =>
            UpdateOverrideHides(category, toggleOn, true);

        private void UpdateOverrideHides(string category, bool toggleOn, bool setAsDirty)
        {
            if (toggleOn)
                model.forceRender.Add(category);
            else
                model.forceRender.Remove(category);

            avatarIsDirty = setAsDirty;
            avatarSlotsHUDController.Recalculate(model.forceRender);
            UpdateAvatarPreview();
        }

        private void OnBackpackVisibleChanged(bool current, bool _) =>
            SetVisibility(current, saveAvatar: avatarIsDirty);

        private void SetVisibility(bool visible, bool saveAvatar = true)
        {
            async UniTaskVoid SetVisibilityAsync(bool visible, bool saveAvatar, CancellationToken cancellationToken)
            {
                if (visible)
                {
                    avatarIsDirty = false;
                    dataStore.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InEditor);
                    backpackEmotesSectionController.RestoreEmoteSlots();
                    backpackEmotesSectionController.LoadEmotes();
                    wearableGridController.LoadWearables();
                    wearableGridController.LoadCollections();
                    LoadUserProfile(ownUserProfile);
                    view.Show();
                    view.SetSignUpModeActive(dataStore.common.isSignUpFlow.Get() && isNewTermsOfServiceAndEmailSubscriptionEnabled);

                    if (dataStore.common.isSignUpFlow.Get())
                    {
                        if (isNewTermsOfServiceAndEmailSubscriptionEnabled)
                            view.HideContinueSignup();
                        else
                            view.ShowContinueSignup();

                        avatarSlotsHUDController.SelectSlot(WearableLiterals.Categories.BODY_SHAPE);
                    }
                    else
                        view.HideContinueSignup();
                }
                else
                {
                    if (saveAvatar)
                        await SaveAsync(cancellationToken);
                    else
                        CloseView();

                    wearableGridController.CancelWearableLoading();
                }
            }

            setVisibilityCancellationToken = setVisibilityCancellationToken.SafeRestart();
            SetVisibilityAsync(visible, saveAvatar, setVisibilityCancellationToken.Token).Forget();
        }

        private void CloseView()
        {
            if (isTakingSnapshot)
                return;

            view.Hide();
            view.ResetPreviewPanel();
            wearableGridController.ResetFilters();
            dataStore.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InWorld);
        }

        private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void SaveBackpackBeforeSignUpFinishes(bool isBackpackWaitingToBeSaved, bool _)
        {
            if (!isBackpackWaitingToBeSaved)
                return;

            view.SetSignUpStage(SignUpStage.CustomizeAvatar);
            saveCancellationToken = saveCancellationToken.SafeRestart();
            SaveAsync(saveCancellationToken.Token).Forget();
        }

        private void LoadUserProfileFromProfileUpdate(UserProfile userProfile)
        {
            bool isEditorVisible = rendererState.Get() && view.isVisible;
            if (!isEditorVisible) return;
            LoadUserProfile(userProfile);
        }

        private void LoadUserProfile(UserProfile userProfile)
        {
            if (avatarIsDirty)
            {
                Debug.LogWarning("Skip the load of the user profile: avatarIsDirty=true");
                return;
            }

            if (userProfile == null)
            {
                Debug.LogWarning("Skip the load of the user profile: userProfile=null");
                return;
            }

            if (userProfile.avatar == null || string.IsNullOrEmpty(userProfile.avatar.bodyShape))
            {
                Debug.LogWarning("Skip the load of the user profile: the avatar body shape is invalid");
                return;
            }

            async UniTaskVoid LoadUserAvatarAsync(AvatarModel avatar, CancellationToken cancellationToken)
            {
                try
                {
                    foreach (AvatarModel.AvatarEmoteEntry emote in avatar.emotes)
                    {
                        string shortenedEmotedUrn = ExtendedUrnParser.GetShortenedUrn(emote.urn);
                        if (!emote.urn.Equals(shortenedEmotedUrn))
                            extendedWearableUrns[shortenedEmotedUrn] = emote.urn;
                    }

                    wearablesCatalogService.WearablesCatalog.TryGetValue(avatar.bodyShape, out var bodyShape);
                    bodyShape ??= await wearablesCatalogService.RequestWearableAsync(avatar.bodyShape, cancellationToken);

                    UnEquipCurrentBodyShape(false);
                    EquipBodyShape(bodyShape, false);

                    model.skinColor = avatar.skinColor;
                    model.hairColor = avatar.hairColor;
                    model.eyesColor = avatar.eyeColor;
                    model.forceRender = new HashSet<string>(avatar.forceRender);
                    model.wearables.Clear();
                    previewEquippedWearables.Clear();

                    int wearablesCount = avatar.wearables.Count;

                    for (var i = 0; i < wearablesCount; i++)
                    {
                        string wearableId = avatar.wearables[i];
                        string shortenedWearableId = ExtendedUrnParser.GetShortenedUrn(wearableId);

                        if (!wearableId.Equals(shortenedWearableId))
                            extendedWearableUrns[shortenedWearableId] = wearableId;

                        if (!wearablesCatalogService.WearablesCatalog.TryGetValue(shortenedWearableId, out WearableItem wearable))
                        {
                            try { wearable = await wearablesCatalogService.RequestWearableAsync(shortenedWearableId, cancellationToken); }
                            catch (OperationCanceledException) { throw; }
                            catch (Exception e)
                            {
                                Debug.LogError($"Cannot load the wearable {shortenedWearableId}");
                                Debug.LogException(e);
                                continue;
                            }
                        }

                        try { EquipWearable(wearableId, wearable, EquipWearableSource.None, false, false, false); }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception e)
                        {
                            Debug.LogError($"Cannot equip the wearable {shortenedWearableId}");
                            Debug.LogException(e);
                        }
                    }

                    avatarSlotsHUDController.Recalculate(model.forceRender);
                    UpdateAvatarModel(model.ToAvatarModel());
                }
                catch (OperationCanceledException) { Debug.LogWarning("Skip the load of the user profile: the operation has been cancelled"); }
                catch (Exception e) { Debug.LogException(e); }
            }

            loadProfileCancellationToken = loadProfileCancellationToken.SafeRestart();
            LoadUserAvatarAsync(userProfile.avatar, loadProfileCancellationToken.Token).Forget();
        }

        private void UpdateAvatarPreview()
        {
            AvatarModel modelToUpdate = model.ToAvatarModel();

            // We always keep the loaded emotes into the Avatar Preview
            foreach (string emoteId in dataStore.emotesCustomization.currentLoadedEmotes.Get())
                modelToUpdate.emotes.Add(new AvatarModel.AvatarEmoteEntry
                    { urn = emoteId });

            UpdateAvatarModel(modelToUpdate);
        }

        private void OnNewEmoteAdded(string emoteId) =>
            UpdateAvatarPreview();

        private void OnEmotePreviewed(string emoteId) =>
            view.PlayPreviewEmote(emoteId);

        private void OnEmoteEquipped(string emoteId) =>
            avatarIsDirty = true;

        private void OnEmoteUnEquipped(string emoteId) =>
            avatarIsDirty = true;

        private void EquipBodyShape(WearableItem bodyShape, bool setAsDirty = true)
        {
            if (bodyShape.data.category != WearableLiterals.Categories.BODY_SHAPE)
            {
                Debug.LogError($"Item ({bodyShape.id} is not a body shape");
                return;
            }

            model.bodyShape = bodyShape;
            dataStore.backpackV2.previewBodyShape.Set(bodyShape.id);
            avatarSlotsHUDController.Equip(bodyShape, bodyShape.id, model.forceRender);
            backpackEmotesSectionController.SetEquippedBodyShape(bodyShape.id);
            wearableGridController.Equip(bodyShape.id);
            wearableGridController.UpdateBodyShapeCompatibility(bodyShape.id);

            if (setAsDirty)
                avatarIsDirty = true;
        }

        private void SaveAvatarAndContinueSignupProcess()
        {
            if (!isNewTermsOfServiceAndEmailSubscriptionEnabled)
            {
                SetVisibility(false, saveAvatar: true);
                return;
            }

            dataStore.HUDs.signupVisible.Set(true);
            view.SetSignUpStage(SignUpStage.SetNameAndEmail);
            view.SetAvatarPreviewFocus(PreviewCameraFocus.FaceEditing);
            view.PlayPreviewEmote(EMOTE_ID);
            backpackAnalyticsService.SendAvatarEditSuccessNuxAnalytic();
        }

        private async UniTask SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await TakeSnapshotsAndSaveAvatarAsync(cancellationToken);

                if (dataStore.backpackV2.isWaitingToBeSavedAfterSignUp.Get())
                    dataStore.backpackV2.isWaitingToBeSavedAfterSignUp.Set(false);

                CloseView();
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
                CloseView();
            }
        }

        private UniTask TakeSnapshotsAndSaveAvatarAsync(CancellationToken cancellationToken)
        {
            UniTaskCompletionSource task = new ();
            isTakingSnapshot = true;

            TakeSnapshots(
                onSuccess: (face256Snapshot, bodySnapshot) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SaveAvatar(face256Snapshot, bodySnapshot);
                    isTakingSnapshot = false;
                    task.TrySetResult();
                },
                onFailed: () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SaveAvatar(new Texture2D(256, 256), new Texture2D(256, 256));
                    isTakingSnapshot = false;
                    task.TrySetException(new Exception("Error taking avatar screenshots."));
                });

            return task.Task;
        }

        private void TakeSnapshots(IBackpackEditorHUDView.OnSnapshotsReady onSuccess, Action onFailed)
        {
            view.TakeSnapshotsAfterStopPreviewAnimation(
                onSuccess: (face256Snapshot, bodySnapshot) => onSuccess?.Invoke(face256Snapshot, bodySnapshot),
                onFailed: () => onFailed?.Invoke());
        }

        private void SaveAvatar(Texture2D face256Snapshot, Texture2D bodySnapshot)
        {
            var avatarModel = model.ToAvatarModel();

            // Restore extended urns
            for (var i = 0; i < avatarModel.wearables.Count; i++)
            {
                string shortenedUrn = avatarModel.wearables[i];

                if (extendedWearableUrns.TryGetValue(shortenedUrn, out string urn))
                    avatarModel.wearables[i] = urn;
            }

            // Add the equipped emotes to the avatar model
            List<AvatarModel.AvatarEmoteEntry> emoteEntries = new List<AvatarModel.AvatarEmoteEntry>();
            int equippedEmotesCount = dataStore.emotesCustomization.unsavedEquippedEmotes.Count();

            for (var i = 0; i < equippedEmotesCount; i++)
            {
                var equippedEmote = dataStore.emotesCustomization.unsavedEquippedEmotes[i];

                if (equippedEmote == null)
                    continue;

                string id = equippedEmote.id;

                if (extendedWearableUrns.TryGetValue(id, out string urn))
                    id = urn;
                else if (emotesCatalogService.TryGetOwnedUrn(id, out string extendedUrn))
                    if (!string.IsNullOrEmpty(extendedUrn))
                        id = extendedUrn;

                emoteEntries.Add(new AvatarModel.AvatarEmoteEntry { slot = i, urn = id });
            }

            avatarModel.emotes = emoteEntries;

            dataStore.emotesCustomization.equippedEmotes.Set(dataStore.emotesCustomization.unsavedEquippedEmotes.Get());

            userProfileBridge.SendSaveAvatar(avatarModel, face256Snapshot, bodySnapshot, dataStore.common.isSignUpFlow.Get());
            ownUserProfile.OverrideAvatar(avatarModel, face256Snapshot);

            if (dataStore.common.isSignUpFlow.Get())
            {
                dataStore.HUDs.signupVisible.Set(true);

                if (!isNewTermsOfServiceAndEmailSubscriptionEnabled)
                    backpackAnalyticsService.SendAvatarEditSuccessNuxAnalytic();
            }

            avatarIsDirty = false;
        }

        private void SelectWearable(string wearableId)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot pre-visualize wearable {wearableId}");
                return;
            }
        }

        private void EquipWearableFromGrid(string wearableId, EquipWearableSource source)
        {
            AudioScriptableObjects.equip.Play(true);
            EquipWearable(wearableId, source);
        }

        private void UnEquipWearableFromGrid(string wearableId, UnequipWearableSource source)
        {
            AudioScriptableObjects.unequip.Play(true);
            UnEquipWearable(wearableId, source);
        }

        private void OnCategoryFilterRemoved() =>
            view.SetColorPickerVisibility(false);

        private void UnEquipWearableFromSlot(string wearableId, UnequipWearableSource source)
        {
            AudioScriptableObjects.unequip.Play(true);
            UnEquipWearable(wearableId, source);
        }

        private void EquipWearable(string wearableId,
            EquipWearableSource source,
            bool setAsDirty = true,
            bool updateAvatarPreview = true,
            bool resetOverride = true)
        {
            string shortenedWearableId = ExtendedUrnParser.GetShortenedUrn(wearableId);

            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(shortenedWearableId, out WearableItem wearable))
            {
                if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out wearable))
                {
                    Debug.LogError($"Cannot equip wearable {shortenedWearableId}");
                    return;
                }
            }

            EquipWearable(wearableId, wearable, source, setAsDirty, updateAvatarPreview, resetOverride);
        }

        private void EquipWearable(
            string extendedWearableId,
            WearableItem wearable,
            EquipWearableSource source,
            bool setAsDirty = true,
            bool updateAvatarPreview = true,
            bool resetOverride = true)
        {
            string shortenWearableId = ExtendedUrnParser.GetShortenedUrn(wearable.id);

            if (ExtendedUrnParser.IsExtendedUrn(extendedWearableId))
                extendedWearableUrns[shortenWearableId] = extendedWearableId;

            if (wearable.data.category == WearableLiterals.Categories.BODY_SHAPE)
            {
                UnEquipCurrentBodyShape();
                EquipBodyShape(wearable);
                ReplaceIncompatibleWearablesWithDefaultWearables();
            }
            else
            {
                foreach (var w in model.wearables.Values)
                {
                    if (w.data.category != wearable.data.category)
                        continue;

                    UnEquipWearable(w.id, UnequipWearableSource.None);
                    break;
                }

                model.wearables.Add(shortenWearableId, wearable);
                previewEquippedWearables.Add(shortenWearableId);

                if (resetOverride)
                    ResetOverridesOfAffectedCategories(wearable, setAsDirty);

                avatarSlotsHUDController.Equip(wearable, model.bodyShape.id, model.forceRender);
                wearableGridController.Equip(shortenWearableId);
            }

            if (setAsDirty)
                avatarIsDirty = true;

            if (source != EquipWearableSource.None)
                backpackAnalyticsService.SendEquipWearableAnalytic(wearable.id, wearable.data.category, wearable.rarity, source);

            if (updateAvatarPreview)
            {
                UpdateAvatarModel(model.ToAvatarModel());
                categoryPendingToPlayEmote = wearable.data.category;
            }

            if (wearable.data.blockVrmExport && !vrmBlockingWearables.ContainsKey(shortenWearableId))
            {
                vrmBlockingWearables.Add(shortenWearableId, wearable.CanBeUnEquipped());
                UpdateVRMExportWarning();
            }
        }

        private void ReplaceIncompatibleWearablesWithDefaultWearables()
        {
            WearableItem bodyShape = model.bodyShape;

            if (bodyShape == null) return;
            if (!fallbackWearables.ContainsKey(bodyShape.id)) return;

            HashSet<string> replacedCategories = new ();

            foreach (var w in model.wearables.Values.ToArray())
            {
                if (w.SupportsBodyShape(bodyShape.id)) continue;

                UnEquipWearable(w, UnequipWearableSource.None, true, false);

                string category = w.data.category;

                if (!string.IsNullOrEmpty(category))
                    replacedCategories.Add(category);
            }

            Dictionary<string, string> fallbackWearablesByCategory = fallbackWearables[bodyShape.id];

            foreach (string category in replacedCategories)
            {
                if (!fallbackWearablesByCategory.ContainsKey(category)) continue;
                EquipWearable(fallbackWearablesByCategory[category], EquipWearableSource.None, true, false);
            }
        }

        private void UnEquipCurrentBodyShape(bool setAsDirty = true)
        {
            if (model.bodyShape.id == NOT_LOADED) return;

            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(model.bodyShape.id, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip body shape {model.bodyShape.id}");
                return;
            }

            UnEquipWearable(wearable, UnequipWearableSource.None, setAsDirty);
        }

        private void UnEquipWearable(string wearableId, UnequipWearableSource source, bool setAsDirty = true, bool updateAvatarPreview = true)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip wearable {wearableId}");
                return;
            }

            UnEquipWearable(wearable, source, setAsDirty, updateAvatarPreview);
        }

        private void UnEquipWearable(WearableItem wearable,
            UnequipWearableSource source = UnequipWearableSource.None,
            bool setAsDirty = true,
            bool updateAvatarPreview = true)
        {
            string shortenedWearableId = ExtendedUrnParser.GetShortenedUrn(wearable.id);

            if (source != UnequipWearableSource.None)
                backpackAnalyticsService.SendUnequippedWearableAnalytic(wearable.id, wearable.data.category, wearable.rarity, source);

            ResetOverridesOfAffectedCategories(wearable, setAsDirty);

            avatarSlotsHUDController.UnEquip(wearable.data.category, model.forceRender);
            model.wearables.Remove(shortenedWearableId);
            previewEquippedWearables.Remove(shortenedWearableId);
            wearableGridController.UnEquip(shortenedWearableId);

            if (setAsDirty)
                avatarIsDirty = true;

            if (updateAvatarPreview)
                UpdateAvatarModel(model.ToAvatarModel());

            if (wearable.data.blockVrmExport)
            {
                if (vrmBlockingWearables.Remove(shortenedWearableId))
                    UpdateVRMExportWarning();
            }
        }

        private void UpdateVRMExportWarning()
        {
            bool vrmWarningEnabled = vrmBlockingWearables.Count > 0;
            view.SetWarningForVRMExportButton(vrmWarningEnabled);
        }

        private void ResetOverridesOfAffectedCategories(WearableItem wearable, bool setAsDirty = true)
        {
            if (wearable.GetHidesList(ownUserProfile.avatar.bodyShape) != null)
                foreach (string s in wearable.GetHidesList(ownUserProfile.avatar.bodyShape))
                    UpdateOverrideHides(s, false, setAsDirty);
        }

        private void ToggleSlot(string slotCategory, bool supportColor, PreviewCameraFocus previewCameraFocus, bool isSelected)
        {
            currentSlotSelected = isSelected ? slotCategory : null;
            view.UpdateHideUnhideStatus(currentSlotSelected, model.forceRender);
            view.SetColorPickerVisibility(isSelected && supportColor);

            if (isSelected && supportColor)
                view.SetColorPickerAsSkinMode(slotCategory == WearableLiterals.Categories.BODY_SHAPE);

            switch (slotCategory)
            {
                case WearableLiterals.Categories.EYES:
                    view.SetColorPickerValue(model.eyesColor);
                    break;
                case WearableLiterals.Categories.HAIR or WearableLiterals.Categories.EYEBROWS or WearableLiterals.Categories.FACIAL_HAIR:
                    view.SetColorPickerValue(model.hairColor);
                    break;
                case WearableLiterals.Categories.BODY_SHAPE:
                    view.SetColorPickerValue(model.skinColor);
                    break;
            }

            view.SetAvatarPreviewFocus(currentSlotSelected != null ? previewCameraFocus : PreviewCameraFocus.DefaultEditing);
        }

        private void OnWearableColorChanged(Color newColor)
        {
            var colorChanged = false;

            switch (currentSlotSelected)
            {
                case WearableLiterals.Categories.EYES:
                    model.eyesColor = newColor;
                    colorChanged = true;
                    break;
                case WearableLiterals.Categories.HAIR or WearableLiterals.Categories.EYEBROWS or WearableLiterals.Categories.FACIAL_HAIR:
                    model.hairColor = newColor;
                    colorChanged = true;
                    break;
                case WearableLiterals.Categories.BODY_SHAPE:
                    model.skinColor = newColor;
                    colorChanged = true;
                    break;
            }

            if (!colorChanged)
                return;

            avatarIsDirty = true;
            UpdateAvatarModel(model.ToAvatarModel());
        }

        private void SetAllColors(Color eyesColor, Color hairColor, Color bodyColor)
        {
            model.eyesColor = eyesColor;
            model.hairColor = hairColor;
            model.skinColor = bodyColor;

            avatarIsDirty = true;
            UpdateAvatarModel(model.ToAvatarModel());
        }

        private void UpdateAvatarModel(AvatarModel avatarModel)
        {
            view.UpdateAvatarPreview(avatarModel);
            outfitsController.UpdateAvatarPreview(model.ToAvatarModel(extendedWearableUrns));
        }

        private void OnColorPickerToggled() =>
            backpackAnalyticsService.SendAvatarColorPick();

        private void OnAvatarUpdated()
        {
            if (string.IsNullOrEmpty(categoryPendingToPlayEmote))
                return;

            PlayEquipAnimation(categoryPendingToPlayEmote);
            categoryPendingToPlayEmote = null;
        }

        private void PlayEquipAnimation(string category)
        {
            view.PlayPreviewEmote(
                GetEquipEmoteByCategory(category),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        private string GetEquipEmoteByCategory(string category)
        {
            string equipEmote = category switch
                                {
                                    WearableLiterals.Categories.FEET => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_SHOES, 2),
                                    WearableLiterals.Categories.LOWER_BODY => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_LOWER, 3),
                                    WearableLiterals.Categories.UPPER_BODY => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_UPPER, 3),
                                    WearableLiterals.Categories.EYEWEAR => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.TIARA => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.EARRING => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.HAT => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.TOP_HEAD => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.HELMET => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.MASK => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.SKIN => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_UPPER, 3),
                                    _ => string.Empty,
                                };

            return equipEmote;
        }

        private string GetRandomizedName(string baseString, int limit)
        {
            currentAnimationIndexShown = (currentAnimationIndexShown + 1) % limit;
            return $"{baseString}{(currentAnimationIndexShown + 1).ToString()}";
        }

        private void OnVrmExport()
        {
            if(vrmBlockingWearables.Count > 0) return;

            vrmExportCts?.SafeCancelAndDispose();
            vrmExportCts = new CancellationTokenSource();
            VrmExport(vrmExportCts.Token).Forget();
        }

        private void OnSignUpBack(SignUpStage stage)
        {
            switch (stage)
            {
                default:
                case SignUpStage.CustomizeAvatar:
                    userProfileBridge.LogOut();
                    break;
                case SignUpStage.SetNameAndEmail:
                    dataStore.HUDs.signupVisible.Set(false);
                    view.SetSignUpStage(SignUpStage.CustomizeAvatar);
                    view.SetAvatarPreviewFocus(PreviewCameraFocus.DefaultEditing);
                    break;
            }
        }

        internal async UniTask VrmExport(CancellationToken ct)
        {
            const int SUCCESS_TOAST_ACTIVE_TIME = 2000;

            try
            {
                view?.SetVRMButtonEnabled(false);
                view?.SetVRMSuccessToastActive(false);

                backpackAnalyticsService.SendVRMExportStarted();

                StringBuilder reference = new StringBuilder();

                try
                {
                    var wearables = await this.ownUserProfile.avatar.wearables.Select(x => this.wearablesCatalogService.RequestWearableAsync(x, ct));

                    foreach (WearableItem wearableItem in wearables)
                    {
                        reference.AppendLine(string.Join(":",
                            wearableItem.data.category,
                            wearableItem.GetName(),
                            wearableItem.GetMarketplaceLink()
                        ));
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                byte[] bytes = await vrmExporter.Export($"{this.ownUserProfile.userName} Avatar", reference.ToString(), view?.originalVisibleRenderers, ct);

                string fileName = $"{this.ownUserProfile.userName.Replace("#", "_")}_{DateTime.Now.ToString("yyyyMMddhhmmss")}";
                await fileBrowser.SaveFileAsync("Save your VRM", Application.persistentDataPath, fileName, bytes, new ExtensionFilter("vrm", "vrm"));

                view?.SetVRMSuccessToastActive(true);
                await UniTask.Delay(SUCCESS_TOAST_ACTIVE_TIME, cancellationToken: ct);
            }
            catch (OperationCanceledException) { }
            finally
            {
                view?.SetVRMButtonEnabled(true);
                view?.SetVRMSuccessToastActive(false);
            }

            //backpackAnalyticsService.SendVRMExportSucceeded();
        }
    }
}
