using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Quests
{
    public class ActiveQuestComponentView : BaseComponentView<ActiveQuestComponentModel>, IActiveQuestComponentView, IPointerClickHandler
    {
        [SerializeField] internal TMP_Text questName;
        [SerializeField] internal TMP_Text questCreator;
        [SerializeField] internal GameObject pinnedIcon;
        [SerializeField] internal GameObject focusOutline;
        [SerializeField] internal GameObject selectedOutline;
        [SerializeField] internal Image background;
        [SerializeField] internal ImageComponentView backgroundImage;

        [SerializeField] internal Color deselectedNameColor;
        [SerializeField] internal Color selectedNameColor;
        [SerializeField] internal Color deselectedCreatorColor;
        [SerializeField] internal Color selectedCreatorColor;
        [SerializeField] internal Color selectedBackgroundColor;
        [SerializeField] internal Color deselectedBackgroundColor;

        public event Action<string> OnActiveQuestSelected;

        internal bool isSelected = false;

        public override void RefreshControl()
        {
            Deselect();
            SetQuestName(model.questName);
            SetQuestCreator(model.questCreator);
            SetQuestId(model.questId);
            SetIsPinned(model.isPinned);
            SetQuestImage(model.questImageUri);
        }

        public void SetQuestName(string title)
        {
            model.questName = title;
            questName.text = title;
        }

        public void SetQuestCreator(string creator)
        {
            model.questCreator = creator;
            questCreator.text = creator;
        }

        public void SetQuestId(string questId) =>
            model.questId = questId;

        public void SetIsPinned(bool isPinned)
        {
            model.isPinned = isPinned;
            pinnedIcon.SetActive(isPinned);
        }

        public void SetQuestImage(string imageUri)
        {
            model.questImageUri = imageUri;
            backgroundImage.SetImage(imageUri);
        }

        public void Deselect()
        {
            isSelected = false;
            selectedOutline.SetActive(false);
            questName.color = deselectedNameColor;
            questCreator.color = deselectedCreatorColor;
            background.color = deselectedBackgroundColor;
        }

        public override void OnFocus()
        {
            if (isSelected) return;

            focusOutline.SetActive(true);
        }

        public override void OnLoseFocus() =>
            focusOutline.SetActive(false);

        public void OnPointerClick(PointerEventData eventData)
        {
            OnActiveQuestSelected?.Invoke(model.questId);
            isSelected = true;
            focusOutline.SetActive(false);
            selectedOutline.SetActive(true);
            questName.color = selectedNameColor;
            questCreator.color = selectedCreatorColor;
            background.color = selectedBackgroundColor;
        }
    }
}
