﻿using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public abstract class ChatEntry : MonoBehaviour
    {
        [SerializeField] internal CanvasGroup group;

        public abstract string DateString { get; }

        public abstract event Action<ChatEntry> OnUserNameClicked;
        public abstract event Action<ChatEntry> OnTriggerHover;
        public abstract event Action<ChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
        public abstract event Action OnCancelHover;
        public abstract event Action OnCancelGotoHover;

        public abstract ChatEntryModel Model { get; }
        public abstract void Populate(ChatEntryModel model);
        public abstract void SetFadeout(bool enabled);
        public abstract void DockContextMenu(RectTransform panel);
        public abstract void DockHoverPanel(RectTransform panel);
        public abstract void ConfigureMentionLinkDetector(UserContextMenu userContextMenu);
    }
}
