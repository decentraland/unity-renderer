using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public abstract class ChatEntry : MonoBehaviour
    {
        [SerializeField] internal CanvasGroup group;

        public abstract string HoverString { get; }

        public abstract event Action<ChatEntry> OnUserNameClicked;
        public abstract event Action<ChatEntry> OnTriggerHover;
        public abstract event Action<ChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
        public abstract event Action OnCancelHover;
        public abstract event Action OnCancelGotoHover;
        public abstract event Action<ChatEntry> OnCopyClicked;

        public abstract ChatEntryModel Model { get; }
        public abstract void Populate(ChatEntryModel model);
        public abstract void SetFadeout(bool enabled);
        public abstract void DockContextMenu(RectTransform panel);
        public abstract void DockHoverPanel(RectTransform panel);
        public abstract void ConfigureMentionLinkDetector(UserContextMenu userContextMenu);
    }
}
