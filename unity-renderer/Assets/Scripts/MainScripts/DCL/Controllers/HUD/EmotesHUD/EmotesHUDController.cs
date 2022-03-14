using DCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmotesCustomization
{
    public class EmotesHUDController : IHUD
    {
        internal EmotesHUDView view;
        private BaseVariable<bool> emotesVisible => DataStore.i.HUDs.emotesVisible;
        private BaseVariable<bool> isAvatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
        private BaseVariable<bool> isStartMenuOpen => DataStore.i.exploreV2.isOpen;
        private BaseVariable<bool> canStartMenuBeOpened => DataStore.i.exploreV2.isSomeModalOpen;
        private bool shortcutsCanBeUsed => !isStartMenuOpen.Get();
        private BaseVariable<bool> isEmotesCustomizationSelected => DataStore.i.emotesCustomization.isEmotesCustomizationSelected;
        private BaseCollection<EquippedEmoteData> equippedEmotes => DataStore.i.emotesCustomization.equippedEmotes;

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
        private bool emoteJustTriggeredFromShortcut = false;
        private UserProfile userProfile;
        private BaseDictionary<string, WearableItem> catalog;
        private bool ownedWearablesAlreadyRequested = false;

        public EmotesHUDController(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog)
        {
            view = EmotesHUDView.Create();
            view.OnClose += OnViewClosed;
            view.onEmoteClicked += PlayEmote;
            view.OnCustomizeClicked += OpenEmotesCustomizationSection;

            ownUserProfile.OnAvatarExpressionSet += OnAvatarEmoteSet;
            emotesVisible.OnChange += OnEmoteVisibleChanged;
            OnEmoteVisibleChanged(emotesVisible.Get(), false);

            isStartMenuOpen.OnChange += IsStartMenuOpenChanged;

            this.userProfile = userProfile;
            this.catalog = catalog;
            equippedEmotes.OnSet += OnEquippedEmotesSet;
            OnEquippedEmotesSet(equippedEmotes.Get());

            ConfigureShortcuts();
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
            List<EmotesHUDView.EmoteSlotData> emotesToSet = new List<EmotesHUDView.EmoteSlotData>();
            foreach (EquippedEmoteData equippedEmoteData in equippedEmotes.Get())
            {
                if (equippedEmoteData != null)
                {
                    catalog.TryGetValue(equippedEmoteData.id, out WearableItem emoteItem);

                    if (emoteItem != null)
                    {
                        if (!emoteItem.data.tags.Contains("base-wearable") && userProfile.GetItemAmount(emoteItem.id) == 0)
                        {
                            emotesToSet.Add(null);
                        }
                        else
                        {
                            emotesToSet.Add(new EmotesHUDView.EmoteSlotData
                            {
                                emoteItem = emoteItem,
                                thumbnailSprite = equippedEmoteData.cachedThumbnail
                            });
                        }
                    }
                    else
                    {
                        emotesToSet.Add(null);
                    }
                }
                else
                {
                    emotesToSet.Add(null);
                }
            }

            view.SetEmotes(emotesToSet);
        }

        public void SetVisibility_Internal(bool visible)
        {
            if (emoteJustTriggeredFromShortcut)
            {
                emoteJustTriggeredFromShortcut = false;
                emotesVisible.Set(false);
                return;
            }

            view.SetVisiblity(visible);

            if (visible)
            {
                DCL.Helpers.Utils.UnlockCursor();

                if (userProfile != null &&
                    !string.IsNullOrEmpty(userProfile.userId) && 
                    !ownedWearablesAlreadyRequested)
                {
                    CatalogController.RequestOwnedWearables(userProfile.userId)
                        .Then((ownedWearables) =>
                        {
                            ownedWearablesAlreadyRequested = true;
                            userProfile.SetInventory(ownedWearables.Select(x => x.id).ToArray());
                            UpdateEmoteSlots();
                        });
                }
            }

            canStartMenuBeOpened.Set(visible);
        }

        public void Dispose()
        {
            view.OnClose -= OnViewClosed;
            view.onEmoteClicked -= PlayEmote;
            view.OnCustomizeClicked -= OpenEmotesCustomizationSection;
            closeWindow.OnTriggered -= OnCloseWindowPressed;
            ownUserProfile.OnAvatarExpressionSet -= OnAvatarEmoteSet;
            emotesVisible.OnChange -= OnEmoteVisibleChanged;
            equippedEmotes.OnSet -= OnEquippedEmotesSet;
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

            if (view != null)
            {
                view.CleanUp();
                UnityEngine.Object.Destroy(view.gameObject);
            }
        }

        public void PlayEmote(string id) 
        {
            if (string.IsNullOrEmpty(id))
                return;

            UserProfile.GetOwnUserProfile().SetAvatarExpression(id);
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
        }

        private void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            if (!shortcutsCanBeUsed)
                return;

            switch (action)
            {
                case DCLAction_Trigger.ToggleEmoteShortcut0:
                    PlayEmote(equippedEmotes[0]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut1:
                    PlayEmote(equippedEmotes[1]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut2:
                    PlayEmote(equippedEmotes[2]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut3:
                    PlayEmote(equippedEmotes[3]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut4:
                    PlayEmote(equippedEmotes[4]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut5:
                    PlayEmote(equippedEmotes[5]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut6:
                    PlayEmote(equippedEmotes[6]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut7:
                    PlayEmote(equippedEmotes[7]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut8:
                    PlayEmote(equippedEmotes[8]?.id);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut9:
                    PlayEmote(equippedEmotes[9]?.id);
                    break;
            }

            emoteJustTriggeredFromShortcut = true;
        }

        private void OnViewClosed() { emotesVisible.Set(false); }
        private void OnAvatarEmoteSet(string id, long timestamp) { emotesVisible.Set(false); }
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
            isEmotesCustomizationSelected.Set(true);
        }
    }
}