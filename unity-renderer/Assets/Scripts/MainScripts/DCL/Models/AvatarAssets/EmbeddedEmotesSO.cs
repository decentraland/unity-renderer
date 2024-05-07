using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Emotes
{
    [CreateAssetMenu(menuName = "DCL/Emotes/EmbeddedEmotes", fileName = "EmbeddedEmotes")]
    public class EmbeddedEmotesSO : ScriptableObject
    {
        [SerializeField] private EmbeddedEmote[] emotes;
        [SerializeField] private ExtendedEmote[] extendedEmotes;

        private string[] ids;
        private List<EmbeddedEmote> items;

        public string[] GetAllIds()
        {
            return ids ??= emotes.Select(e => e.id).Union(extendedEmotes.Select(e => e.id)).ToArray();
        }

        public IEnumerable<EmbeddedEmote> GetAllEmotes()
        {
            if (items is { Count: > 0 }) return items;

            items = new List<EmbeddedEmote>();
            items.AddRange(emotes);
            items.AddRange(extendedEmotes);

            return items;
        }

        public EmbeddedEmote[] GetEmbeddedEmotes() =>
            emotes;

        public ExtendedEmote[] GetExtendedEmbeddedEmotes() =>
            extendedEmotes;

        public void Clear()
        {
            emotes = Array.Empty<EmbeddedEmote>();
            extendedEmotes = Array.Empty<ExtendedEmote>();
        }

        public void OverrideEmotes(EmbeddedEmote[] emotes)
        {
            this.emotes = emotes;
        }
    }

    [Serializable]
    public class EmbeddedEmote : WearableItem
    {
        public AnimationClip femaleAnimation;
        public AnimationClip maleAnimation;
        public bool dontShowInBackpack;

        public override bool ShowInBackpack() =>
            !dontShowInBackpack;
    }

    [Serializable]
    public class ExtendedEmote : EmbeddedEmote
    {
        public GameObject propPrefab;
        public AudioClip clip;
    }
}
