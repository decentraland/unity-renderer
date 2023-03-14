using DCL.Chat.HUD;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PrivateChatHUDView : ChatHUDView
    {
        [SerializeField] private PoolPrivateChatEntryFactory chatEntryFactory;

        private readonly Dictionary<string, DateSeparatorEntry> dateSeparators = new Dictionary<string, DateSeparatorEntry>();

        public override void Awake()
        {
            base.Awake();
            ChatEntryFactory = chatEntryFactory;
        }

        public override void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
        {
            AddSeparatorEntryIfNeeded(model);
            base.AddEntry(model, setScrollPositionToBottom);
        }

        public override void ClearAllEntries()
        {
            base.ClearAllEntries();
            dateSeparators.Clear();
        }

        private void AddSeparatorEntryIfNeeded(ChatEntryModel chatEntryModel)
        {
            var entryDateTime = DateTimeOffset.FromUnixTimeMilliseconds((long) chatEntryModel.timestamp).Date;
            var separatorId = entryDateTime.Ticks.ToString();
            if (dateSeparators.ContainsKey(separatorId)) return;
            var dateSeparatorEntry = chatEntryFactory.CreateDateSeparator();
            Dock(dateSeparatorEntry);
            dateSeparatorEntry.Populate(chatEntryModel);
            dateSeparatorEntry.SetFadeout(IsFadeoutModeEnabled);
            dateSeparators[separatorId] = dateSeparatorEntry;
            SetEntry(separatorId, dateSeparatorEntry);
        }
    }
}
