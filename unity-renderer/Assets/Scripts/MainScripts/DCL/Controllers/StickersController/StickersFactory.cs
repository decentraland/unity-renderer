using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StickersFactory", menuName = "Variables/StickersFactory")]
public class StickersFactory : ScriptableObject
{
    [Serializable]
    public class StickerFactoryEntry
    {
        public string id;
        public GameObject stickerPrefab;
    }

    [SerializeField] private List<StickerFactoryEntry> stickersList = new List<StickerFactoryEntry>();
    private Dictionary<string, GameObject> stickers;

    private void EnsureDict()
    {
        if (stickers != null)
            return;

        stickers = new Dictionary<string, GameObject>();
        for (int i = 0; i < stickersList.Count; i++)
        {
            stickers.Add(stickersList[i].id, stickersList[i].stickerPrefab);
        }
    }

    public bool TryGet(string id, out GameObject stickerPrefab)
    {
        EnsureDict();

        return stickers.TryGetValue(id, out stickerPrefab);
    }
}