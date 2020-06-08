using System;
using UnityEngine;

/// <summary>
/// Stores the latest open chats of our friends.
/// </summary>
[CreateAssetMenu(fileName = "LatestOpenChatsList", menuName = "LatestOpenChatsList")]
public class LatestOpenChatsList : ListVariable<LatestOpenChatsList.Model>
{
    [Serializable]
    public class Model
    {
        public string userId;
        public ulong lastTimestamp;
    }
}
