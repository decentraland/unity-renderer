using DCL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Emotes
{
    public interface IEmotesCustomizationComponentController : IDisposable
    {
        /// <summary>
        /// Initializes the emotes customization controller.
        /// </summary>
        void Initialize();
    }

    public class EmotesCustomizationComponentController : IEmotesCustomizationComponentController
    {
        internal const int NUMBER_OF_SLOTS = 9;
        private const string PLAYER_PREFS_EQUIPPED_EMOTES_KEY = "EquippedEmotes";

        internal BaseVariable<Transform> isInitialized => DataStore.i.emotes.isInitialized;
        internal BaseCollection<StoredEmote> equippedEmotes => DataStore.i.emotes.equippedEmotes;
        internal BaseVariable<bool> isStarMenuOpen => DataStore.i.exploreV2.isOpen;
        internal bool shortcutsCanBeUsed => isStarMenuOpen.Get() && view.isActive;

        internal IEmotesCustomizationComponentView view;
        internal InputAction_Hold equipInputAction;
        internal InputAction_Hold showInfoInputAction;
        internal InputAction_Trigger shortcut0InputAction;
        internal InputAction_Trigger shortcut1InputAction;
        internal InputAction_Trigger shortcut2InputAction;
        internal InputAction_Trigger shortcut3InputAction;
        internal InputAction_Trigger shortcut4InputAction;
        internal InputAction_Trigger shortcut5InputAction;
        internal InputAction_Trigger shortcut6InputAction;
        internal InputAction_Trigger shortcut7InputAction;
        internal InputAction_Trigger shortcut8InputAction;
        internal InputAction_Trigger shortcut9InputAction;

        public void Initialize()
        {
            view = CreateView();
            view.onEmoteClicked += OnEmoteAnimationRaised;
            view.onEmoteEquipped += OnEmoteEquipped;
            view.onEmoteUnequipped += OnEmoteUnequipped;
            isStarMenuOpen.OnChange += IsStarMenuOpenChanged;
            ConfigureShortcuts();
            LoadMockedEmoteCards();
            LoadEquippedEmoteSlots();

            isInitialized.Set(view.viewTransform);
        }

        public void Dispose()
        {
            view.onEmoteClicked -= OnEmoteAnimationRaised;
            view.onEmoteEquipped -= OnEmoteEquipped;
            view.onEmoteUnequipped -= OnEmoteUnequipped;
            isStarMenuOpen.OnChange -= IsStarMenuOpenChanged;
            equipInputAction.OnFinished -= OnEquipInputActionTriggered;
            showInfoInputAction.OnFinished -= OnShowInfoInputActionTriggered;
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
        }

        internal void LoadEquippedEmoteSlots()
        {
            List<StoredEmote> storedEquippedEmotes = JsonConvert.DeserializeObject<List<StoredEmote>>(PlayerPrefs.GetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY));
            if (storedEquippedEmotes != null)
                equippedEmotes.Set(storedEquippedEmotes);

            for (int i = 0; i < equippedEmotes.Count(); i++)
            {
                if (i > NUMBER_OF_SLOTS)
                    break;

                if (equippedEmotes[i] == null)
                    continue;

                EmoteCardComponentView emoteModel = view.GetEmoteCardById(equippedEmotes[i].id);
                if (emoteModel != null)
                    view.EquipEmote(emoteModel.model.id, emoteModel.model.name, i);
            }
        }

        internal void OnEmoteAnimationRaised(string emoteId)
        {
            Debug.Log("SANTI ---> EMOTE ANIMATION RAISED: " + emoteId);
        }

        internal void OnEmoteEquipped(string emoteId, int slotNumber)
        {
            EmoteSlotCardComponentView equippedSlot = view.GetEmoteSlotCardBySlotNumber(slotNumber);
            if (slotNumber < equippedEmotes.Count())
            {
                equippedEmotes[slotNumber] = new StoredEmote
                {
                    id = equippedSlot.model.emoteId,
                    pictureUri = equippedSlot.model.pictureUri
                };
            }

            PlayerPrefs.SetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY, JsonConvert.SerializeObject(equippedEmotes.Get().ToList()));
        }

        internal void OnEmoteUnequipped(string emoteId, int slotNumber)
        {
            if (slotNumber < equippedEmotes.Count())
                equippedEmotes[slotNumber] = null;

            PlayerPrefs.SetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY, JsonConvert.SerializeObject(equippedEmotes.Get().ToList()));
        }

        internal void IsStarMenuOpenChanged(bool currentIsOpen, bool previousIsOpen)
        {
            view.CloseEmoteInfoPanel();
        }

        internal void ConfigureShortcuts()
        {
            equipInputAction = Resources.Load<InputAction_Hold>("DefaultConfirmAction");
            equipInputAction.OnFinished += OnEquipInputActionTriggered;

            showInfoInputAction = Resources.Load<InputAction_Hold>("ZoomIn");
            showInfoInputAction.OnFinished += OnShowInfoInputActionTriggered;

            shortcut0InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut0");
            shortcut0InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut1InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut1");
            shortcut1InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut2InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut2");
            shortcut2InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut3InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut3");
            shortcut3InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut4InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut4");
            shortcut4InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut5InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut5");
            shortcut5InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut6InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut6");
            shortcut6InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut7InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut7");
            shortcut7InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut8InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut8");
            shortcut8InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut9InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut9");
            shortcut9InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;
        }

        internal void OnEquipInputActionTriggered(DCLAction_Hold action)
        {
            if (!shortcutsCanBeUsed || view.selectedCard == null)
                return;

            if (!view.selectedCard.model.isAssignedInSelectedSlot)
            {
                view.EquipEmote(
                    view.selectedCard.model.id,
                    view.selectedCard.model.name,
                    view.selectedSlot);
            }
            else
            {
                view.UnequipEmote(
                    view.selectedCard.model.id,
                    view.selectedSlot);
            }
        }

        internal void OnShowInfoInputActionTriggered(DCLAction_Hold action)
        {
            if (!shortcutsCanBeUsed || view.selectedCard == null)
                return;

            view.OpenEmoteInfoPanel(
                view.selectedCard.model,
                view.selectedCard.rarityMark.gameObject.activeSelf ? view.selectedCard.rarityMark.color : Color.grey,
                view.selectedCard.emoteInfoAnchor);
        }

        internal void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            if (!shortcutsCanBeUsed || view.selectedCard == null)
                return;

            switch (action)
            {
                case DCLAction_Trigger.ToggleShortcut0:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 0);
                    break;
                case DCLAction_Trigger.ToggleShortcut1:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 1);
                    break;
                case DCLAction_Trigger.ToggleShortcut2:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 2);
                    break;
                case DCLAction_Trigger.ToggleShortcut3:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 3);
                    break;
                case DCLAction_Trigger.ToggleShortcut4:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 4);
                    break;
                case DCLAction_Trigger.ToggleShortcut5:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 5);
                    break;
                case DCLAction_Trigger.ToggleShortcut6:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 6);
                    break;
                case DCLAction_Trigger.ToggleShortcut7:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 7);
                    break;
                case DCLAction_Trigger.ToggleShortcut8:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 8);
                    break;
                case DCLAction_Trigger.ToggleShortcut9:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 9);
                    break;
            }
        }

        internal virtual IEmotesCustomizationComponentView CreateView() => EmotesCustomizationComponentView.Create();

        // ------------- DEBUG ------------------------
        private void LoadMockedEmoteCards()
        {
            List<string> mockedEmoteNames = new List<string>
            {
                "wave",
                "fistpump",
                "dance",
                "raiseHand",
                "clap",
                "money",
                "kiss",
                "robot",
                "tik",
                "hammer",
                "tektonik",
                "dontsee",
                "handsair",
                "shrug",
                "disco",
                "dab",
                "headexplode",
                "dance",
                "snowfall",
                "hohoho",
                "crafting"
            };

            List<string> mockedImages = new List<string>
            {
                "https://cdn.pixabay.com/photo/2020/05/30/20/51/man-5240413_1280.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Girl_silhouette_hand_up.svg/570px-Girl_silhouette_hand_up.svg.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5b/Karate_silhouette.svg/919px-Karate_silhouette.svg.png",
                "https://freesvg.org/img/1538176837.png",
                "https://freesvg.org/img/woman-silhouette-3.png",
                "https://images.squarespace-cdn.com/content/v1/55947ac3e4b0fa882882cd65/1487633176391-U7VZNFVFYFI8KKCKCYP1/NS_0020.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Woman_Silhouette.svg/1200px-Woman_Silhouette.svg.png",
                "https://cdn.pixabay.com/photo/2020/05/30/20/51/man-5240413_1280.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Girl_silhouette_hand_up.svg/570px-Girl_silhouette_hand_up.svg.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5b/Karate_silhouette.svg/919px-Karate_silhouette.svg.png",
                "https://freesvg.org/img/1538176837.png",
                "https://freesvg.org/img/woman-silhouette-3.png",
                "https://images.squarespace-cdn.com/content/v1/55947ac3e4b0fa882882cd65/1487633176391-U7VZNFVFYFI8KKCKCYP1/NS_0020.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Woman_Silhouette.svg/1200px-Woman_Silhouette.svg.png",
                "https://cdn.pixabay.com/photo/2020/05/30/20/51/man-5240413_1280.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Girl_silhouette_hand_up.svg/570px-Girl_silhouette_hand_up.svg.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5b/Karate_silhouette.svg/919px-Karate_silhouette.svg.png",
                "https://freesvg.org/img/1538176837.png",
                "https://freesvg.org/img/woman-silhouette-3.png",
                "https://images.squarespace-cdn.com/content/v1/55947ac3e4b0fa882882cd65/1487633176391-U7VZNFVFYFI8KKCKCYP1/NS_0020.png",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Woman_Silhouette.svg/1200px-Woman_Silhouette.svg.png"
            };

            List<string> mockedRarities = new List<string>
            {
                "rare",
                "epic",
                "legendary",
                "mythic",
                "unique",
                "common",
                ""
            };

            List<EmoteCardComponentModel> mockedEmotes = new List<EmoteCardComponentModel>();
            for (int i = 0; i < mockedImages.Count; i++)
            {
                mockedEmotes.Add(new EmoteCardComponentModel
                {
                    id = mockedEmoteNames[i],
                    name = mockedEmoteNames[i],
                    description = $"Description of the {mockedEmoteNames[i]} emote...",
                    pictureUri = mockedImages[i],
                    isAssignedInSelectedSlot = false,
                    isSelected = false,
                    assignedSlot = -1,
                    rarity = mockedRarities[UnityEngine.Random.Range(0, mockedRarities.Count)],
                    isInL2 = true
                });
            }

            view.SetEmotes(mockedEmotes);
        }
    }
}