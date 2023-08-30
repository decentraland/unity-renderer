using AvatarSystem;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DCL.Emotes;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace DCL.EmotesWheel
{
    public class EmotesWheelController : IHUD
    {
        internal EmotesWheelView view;
        private BaseVariable<bool> emotesVisible => DataStore.i.HUDs.emotesVisible;
        private BaseVariable<bool> emoteJustTriggeredFromShortcut => DataStore.i.HUDs.emoteJustTriggeredFromShortcut;
        private BaseVariable<bool> isAvatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
        private BaseVariable<bool> isStartMenuOpen => DataStore.i.exploreV2.isOpen;
        private BaseVariable<bool> canStartMenuBeOpened => DataStore.i.exploreV2.isSomeModalOpen;
        private bool shortcutsCanBeUsed => !isStartMenuOpen.Get();
        private DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

        private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
        private InputAction_Trigger closeWindow;
        private InputAction_Hold openEmotesCustomizationInputAction;
        private InputAction_Trigger shortcut0InputAction;
        private InputAction_Trigger shortcut1InputAction;
        private InputAction_Trigger shortcut2InputAction;
        private InputAction_Trigger shortcut3InputAction;
        private InputAction_Trigger shortcut4InputAction;
        private InputAction_Trigger shortcut5InputAction;
        private InputAction_Trigger shortcut6InputAction;
        private InputAction_Trigger shortcut7InputAction;
        private InputAction_Trigger shortcut8InputAction;
        private InputAction_Trigger shortcut9InputAction;
        private InputAction_Trigger auxShortcut0InputAction;
        private InputAction_Trigger auxShortcut1InputAction;
        private InputAction_Trigger auxShortcut2InputAction;
        private InputAction_Trigger auxShortcut3InputAction;
        private InputAction_Trigger auxShortcut4InputAction;
        private InputAction_Trigger auxShortcut5InputAction;
        private InputAction_Trigger auxShortcut6InputAction;
        private InputAction_Trigger auxShortcut7InputAction;
        private InputAction_Trigger auxShortcut8InputAction;
        private InputAction_Trigger auxShortcut9InputAction;
        private readonly UserProfile userProfile;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private bool ownedWearablesAlreadyRequested;
        private readonly BaseDictionary<string, EmoteWheelSlot> slotsInLoadingState = new ();
        private CancellationTokenSource cts = new ();
        private readonly IEmotesCatalogService emoteCatalog;
        private readonly IEmotesService emotesService;
        private IAvatar avatar;
        private IAvatarEmotesController emotesController;

        public EmotesWheelController(
            UserProfile userProfile,
            IEmotesService emotesService,
            IWearablesCatalogService wearablesCatalogService)
        {
            this.emotesService = emotesService;
            closeWindow = Resources.Load<InputAction_Trigger>("CloseWindow");
            closeWindow.OnTriggered += OnCloseWindowPressed;

            view = EmotesWheelView.Create();
            view.OnClose += OnViewClosed;
            view.onEmoteClicked += OnEmoteClicked;
            view.OnCustomizeClicked += OpenEmotesCustomizationSection;

            ownUserProfile.OnAvatarEmoteSet += OnAvatarEmoteSet;
            emotesVisible.OnChange += OnEmoteVisibleChanged;
            OnEmoteVisibleChanged(emotesVisible.Get(), false);

            isStartMenuOpen.OnChange += IsStartMenuOpenChanged;

            this.userProfile = userProfile;
            this.wearablesCatalogService = wearablesCatalogService;
            emotesCustomizationDataStore.equippedEmotes.OnSet += OnEquippedEmotesSet;

            ConfigureShortcuts();

            emotesCustomizationDataStore.isWheelInitialized.Set(true);

            DataStore.i.player.ownPlayer.OnChange += OnPlayerSet;
            OnPlayerSet(DataStore.i.player.ownPlayer.Get(), null);
        }

        private void OnPlayerSet(Player current, Player previous)
        {
            if (current == null) return;
            emotesController = current.avatar.GetEmotesController();
            emotesController.OnEmoteEquipped += OnEmoteEquipped;
            emotesController.OnEmoteUnequipped += OnEmoteUnequipped;
            UpdateEmoteSlots();
        }

        private void OnEmoteEquipped(string emoteId, IEmoteReference emoteReference)
        {
            Debug.Log($"[EMOTE WHEEL] emote equipped {emoteId}");
            RefreshSlotLoadingState(emoteId);
        }

        private void OnEmoteUnequipped(string emoteId)
        {
            Debug.Log($"[EMOTE WHEEL] emote UNequipped {emoteId} :(");
        }

        public void SetVisibility(bool visible)
        {
            //TODO once kernel sends visible properly
            //expressionsVisible.Set(visible);
        }

        private void OnEmoteVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }

        private void IsStartMenuOpenChanged(bool current, bool previous)
        {
            if (!current)
                return;

            emotesVisible.Set(false);
        }

        private void OnEquippedEmotesSet(IEnumerable<EquippedEmoteData> equippedEmotes) { UpdateEmoteSlots(); }

        private void UpdateEmoteSlots()
        {
            if (emotesController == null) return;
            List<EmotesWheelView.EmoteSlotData> emotesToSet = new List<EmotesWheelView.EmoteSlotData>();

            var equippedEmotes = ListPool<EquippedEmoteData>.Get();
            equippedEmotes.AddRange(emotesCustomizationDataStore.equippedEmotes.Get());

            foreach (EquippedEmoteData equippedEmoteData in equippedEmotes)
            {
                if (equippedEmoteData != null)
                {
                    if (emotesController.TryGetEquippedEmote(userProfile.avatar.bodyShape, equippedEmoteData.id, out var emote))
                    {
                        var entity = emote.GetEntity();

                        emotesToSet.Add(new EmotesWheelView.EmoteSlotData
                        {
                            emoteId = equippedEmoteData.id,
                            emoteItem = entity,
                            thumbnailSprite = entity.thumbnailSprite != null ? entity.thumbnailSprite : equippedEmoteData.cachedThumbnail,
                        });
                    }
                    else
                        emotesToSet.Add(new EmotesWheelView.EmoteSlotData
                        {
                            emoteId = equippedEmoteData.id,
                            emoteItem = null,
                            thumbnailSprite = equippedEmoteData.cachedThumbnail,
                        });
                } else
                    equippedEmotes.Add(null);
            }

            ListPool<EquippedEmoteData>.Release(equippedEmotes);

            List<EmoteWheelSlot> updatedWheelSlots = view.SetEmotes(emotesToSet);
            foreach (EmoteWheelSlot slot in updatedWheelSlots)
            {
                slot.SetAsLoading(false);
                if (string.IsNullOrEmpty(slot.emoteId))
                    continue;

                slotsInLoadingState.Remove(slot.emoteId);


                slot.SetAsLoading(true);
                slotsInLoadingState.Add(slot.emoteId, slot);

                RefreshSlotLoadingState(slot.emoteId);
            }
        }

        private void RefreshSlotLoadingState(string emoteId)
        {
            if (emotesController.TryGetEquippedEmote(userProfile.avatar.bodyShape, emoteId, out var emote))
            {
                slotsInLoadingState.TryGetValue(emoteId, out EmoteWheelSlot slot);
                if (slot == null) return;
                var entity = emote.GetEntity();
                view.SetupEmoteWheelSlot(slot, new EmotesWheelView.EmoteSlotData
                {
                    emoteId = emoteId,
                    emoteItem = entity,
                    thumbnailSprite = entity.thumbnailSprite,
                });
                slot.SetAsLoading(false);
                slotsInLoadingState.Remove(emoteId);
            }
        }

        public void SetVisibility_Internal(bool visible)
        {
            async UniTaskVoid RequestOwnedWearablesAsync(CancellationToken ct)
            {
                var ownedWearables = await wearablesCatalogService.RequestOwnedWearablesAsync(
                    userProfile.userId,
                    1,
                    int.MaxValue,
                    true,
                    ct);

                ownedWearablesAlreadyRequested = true;
                userProfile.SetInventory(ownedWearables.wearables.Select(x => x.id));
                UpdateEmoteSlots();
            }

            if (visible && isStartMenuOpen.Get())
                return;

            if (emoteJustTriggeredFromShortcut.Get())
            {
                emotesVisible.Set(false);
                return;
            }

            view.SetVisiblity(visible);

            if (visible)
            {
                Helpers.Utils.UnlockCursor();

                if (userProfile != null &&
                    !string.IsNullOrEmpty(userProfile.userId) &&
                    !ownedWearablesAlreadyRequested)
                {
                    cts?.Cancel();
                    cts?.Dispose();
                    cts = new CancellationTokenSource();
                    RequestOwnedWearablesAsync(cts.Token).Forget();
                }
            }

            canStartMenuBeOpened.Set(visible);
        }

        public void Dispose()
        {
            view.OnClose -= OnViewClosed;
            view.onEmoteClicked -= OnEmoteClicked;
            view.OnCustomizeClicked -= OpenEmotesCustomizationSection;
            closeWindow.OnTriggered -= OnCloseWindowPressed;
            ownUserProfile.OnAvatarEmoteSet -= OnAvatarEmoteSet;
            emotesVisible.OnChange -= OnEmoteVisibleChanged;
            emotesCustomizationDataStore.equippedEmotes.OnSet -= OnEquippedEmotesSet;
            shortcut0InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut1InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut2InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut3InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut4InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut5InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut6InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut7InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut8InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut9InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut0InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut1InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut2InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut3InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut4InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut5InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut6InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut7InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut8InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            auxShortcut9InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;

            if (emotesController != null)
            {
                emotesController.OnEmoteEquipped -= OnEmoteEquipped;
                emotesController.OnEmoteUnequipped -= OnEmoteUnequipped;
            }

            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            if (view != null)
            {
                view.CleanUp();
                Utils.SafeDestroy(view.gameObject);
            }
        }

        public void OnEmoteClicked(string id) { PlayEmote(id, UserProfile.EmoteSource.EmotesWheel); }

        public void PlayEmote(string id, UserProfile.EmoteSource source)
        {
            if (string.IsNullOrEmpty(id))
                return;

            UserProfile.GetOwnUserProfile().SetAvatarExpression(id, source);
        }

        private void ConfigureShortcuts()
        {
            closeWindow = Resources.Load<InputAction_Trigger>("CloseWindow");
            closeWindow.OnTriggered += OnCloseWindowPressed;

            openEmotesCustomizationInputAction = Resources.Load<InputAction_Hold>("DefaultConfirmAction");
            openEmotesCustomizationInputAction.OnFinished += OnOpenEmotesCustomizationInputActionTriggered;

            shortcut0InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut0");
            shortcut0InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut1InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut1");
            shortcut1InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut2InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut2");
            shortcut2InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut3InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut3");
            shortcut3InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut4InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut4");
            shortcut4InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut5InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut5");
            shortcut5InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut6InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut6");
            shortcut6InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut7InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut7");
            shortcut7InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut8InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut8");
            shortcut8InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut9InputAction = Resources.Load<InputAction_Trigger>("ToggleEmoteShortcut9");
            shortcut9InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut0InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut0");
            auxShortcut0InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut1InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut1");
            auxShortcut1InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut2InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut2");
            auxShortcut2InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut3InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut3");
            auxShortcut3InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut4InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut4");
            auxShortcut4InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut5InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut5");
            auxShortcut5InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut6InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut6");
            auxShortcut6InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut7InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut7");
            auxShortcut7InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut8InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut8");
            auxShortcut8InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            auxShortcut9InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut9");
            auxShortcut9InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;
        }

        private void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            if (!shortcutsCanBeUsed)
                return;

            switch (action)
            {
                case DCLAction_Trigger.ToggleEmoteShortcut0:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[0]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut1:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[1]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut2:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[2]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut3:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[3]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut4:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[4]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut5:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[5]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut6:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[6]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut7:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[7]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut8:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[8]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut9:
                    PlayEmote(emotesCustomizationDataStore.equippedEmotes[9]?.id, UserProfile.EmoteSource.Shortcut);
                    emoteJustTriggeredFromShortcut.Set(true);
                    break;
                case DCLAction_Trigger.ToggleShortcut0:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[0]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut1:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[1]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut2:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[2]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut3:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[3]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut4:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[4]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut5:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[5]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut6:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[6]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut7:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[7]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut8:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[8]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
                case DCLAction_Trigger.ToggleShortcut9:
                    if (emotesVisible.Get())
                        PlayEmote(emotesCustomizationDataStore.equippedEmotes[9]?.id, UserProfile.EmoteSource.Shortcut);
                    break;
            }
        }

        private void OnViewClosed() { emotesVisible.Set(false); }
        private void OnAvatarEmoteSet(string id, long timestamp, UserProfile.EmoteSource source) { emotesVisible.Set(false); }
        private void OnCloseWindowPressed(DCLAction_Trigger action) { emotesVisible.Set(false); }

        private void OnOpenEmotesCustomizationInputActionTriggered(DCLAction_Hold action)
        {
            if (!emotesVisible.Get())
                return;

            OpenEmotesCustomizationSection();
        }

        private void OpenEmotesCustomizationSection()
        {
            emotesVisible.Set(false);
            isAvatarEditorVisible.Set(true);
            emotesCustomizationDataStore.isEmotesCustomizationSelected.Set(true);
        }
    }
}
