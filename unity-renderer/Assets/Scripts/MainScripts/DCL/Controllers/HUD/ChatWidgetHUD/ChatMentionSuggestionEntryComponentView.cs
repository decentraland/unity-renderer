using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChatMentionSuggestionEntryComponentView : BaseComponentView<ChatMentionSuggestionModel>,
        ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField] internal Button selectButton;
        [SerializeField] internal TMP_Text userNameLabel;
        [SerializeField] internal ImageComponentView faceImage;

        private bool isSelected;

        public event Action<ChatMentionSuggestionModel> OnSubmitted;

        public bool IsSelected
        {
            get
            {
                GameObject selectedGameObject = EventSystem.current.currentSelectedGameObject;
                return selectedGameObject == gameObject || selectedGameObject == selectButton.gameObject
                    || isSelected;
            }
        }

        public override void Awake()
        {
            base.Awake();

            selectButton.onClick.AddListener(() => OnSubmitted?.Invoke(model));
        }

        public override void RefreshControl()
        {
            userNameLabel.text = model.userName;
            faceImage.SetImage(model.imageUrl);
        }

        public void OnSelect(BaseEventData eventData)
        {
            selectButton.OnSelect(eventData);
            isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selectButton.OnDeselect(eventData);
            isSelected = false;
        }

        public void OnSubmit(BaseEventData eventData) =>
            selectButton.OnSubmit(eventData);

        public void Deselect() =>
            OnDeselect(null);

        public void Select() =>
            OnSelect(null);
    }
}
