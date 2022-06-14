using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class LazyLoadingChatControllerMock : MonoBehaviour, IChatController
{
    [SerializeField] private ChatController controller;
    
    public event Action<ChatMessage> OnAddMessage
    {
        add => controller.OnAddMessage += value;
        remove => controller.OnAddMessage -= value;
    }

    public List<ChatMessage> GetAllocatedEntries() => controller.GetAllocatedEntries();

    // called by kernel
    [UsedImplicitly]
    public void AddMessageToChatWindow(string jsonMessage)
    {
    }

    public void Send(ChatMessage message) => controller.Send(message);
    
    public void MarkMessagesAsSeen(string userId)
    {
        // TODO: should we do anything?
    }

    public async UniTask<List<ChatMessage>> GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        await UniTask.Delay(Random.Range(100, 700));
        // TODO: fake messages
        return new List<ChatMessage>();
    }
}