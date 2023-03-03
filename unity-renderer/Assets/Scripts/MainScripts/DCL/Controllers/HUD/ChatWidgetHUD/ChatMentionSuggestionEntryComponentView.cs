using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChatMentionSuggestionEntryComponentView : BaseComponentView<ChatMentionSuggestionModel>
    {
        [SerializeField] protected Button selectButton;
        [SerializeField] protected TMP_Text userNameLabel;
        [SerializeField] protected ImageComponentView faceImage;

        public event Action<ChatMentionSuggestionModel> OnClicked;

        public override void Awake()
        {
            base.Awake();

            selectButton.onClick.AddListener(() => OnClicked?.Invoke(model));
        }

        public override void RefreshControl()
        {
            userNameLabel.text = model.userName;
            faceImage.SetImage(model.imageUrl);
        }
    }
}
