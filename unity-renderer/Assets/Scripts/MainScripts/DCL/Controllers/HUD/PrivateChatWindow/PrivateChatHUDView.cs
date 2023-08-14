using DCL.Social.Chat;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PrivateChatHUDView : ChatHUDView
    {
        [SerializeField] private PoolPrivateChatEntryFactory chatEntryFactory;

        private readonly Dictionary<string, DateSeparatorEntry> dateSeparators = new ();

        public override void Awake()
        {
            base.Awake();
            ChatEntryFactory = chatEntryFactory;
        }

        public override void SetEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
        {
            AddSeparatorEntryIfNeeded(model);
            base.SetEntry(model, setScrollPositionToBottom);
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
            Populate(dateSeparatorEntry, chatEntryModel);
            dateSeparators[separatorId] = dateSeparatorEntry;
            SetEntry(separatorId, dateSeparatorEntry);
        }
    }
}
