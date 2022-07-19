using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Chat.HUD
{
    [CreateAssetMenu(fileName = "ChannelEntryFactory", menuName = "DCL/Social/ChannelEntryFactory")]
    public class ChannelEntryFactory : ScriptableObject
    {
        [Serializable]
        private struct SpecialEntry
        {
            public string channelId;
            public PublicChatEntry prefab;
        }
        
        [SerializeField] private PublicChatEntry defaultEntryPrefab;
        [SerializeField] private SpecialEntry[] specialEntryPrefabs;

        private Dictionary<string, PublicChatEntry> specialEntries;

        public PublicChatEntry Create(string channelId)
        {
            specialEntries ??= specialEntryPrefabs.ToDictionary(entry => entry.channelId, entry => entry.prefab);
            return Instantiate(specialEntries.ContainsKey(channelId) ? specialEntries[channelId] : defaultEntryPrefab);
        }
    }
}