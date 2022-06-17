using Cysharp.Threading.Tasks;
using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChatController))]
public class LazyLoadingChatControllerMock : IChatController
{
    private ChatController controller;

    public LazyLoadingChatControllerMock(ChatController controller)
    {
        this.controller = controller;

        CreateFakeUsersInCatalog();
    }

    public event Action<ChatMessage> OnAddMessage
    {
        add => controller.OnAddMessage += value;
        remove => controller.OnAddMessage -= value;
    }

    public List<ChatMessage> GetAllocatedEntries() => controller.GetAllocatedEntries();

    public void Send(ChatMessage message) => controller.Send(message);
    
    public void MarkMessagesAsSeen(string userId) { controller.MarkMessagesAsSeen(userId); }

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp) { SimulateDelayedResponseFor_GetPrivateMessages(userId, limit, fromTimestamp); }

    private async UniTask SimulateDelayedResponseFor_GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        await UniTask.Delay(UnityEngine.Random.Range(1000, 3000));

        long fromTimestampToApply = fromTimestamp;

        for (int i = 0; i < limit; i++)
        {
            await UniTask.DelayFrame(1);

            controller.AddMessageToChatWindow(
                CreateMockedDataFor_AddMessageToChatWindowPayload(
                    userId, 
                    $"fake message {i + 1} from user {userId}",
                    fromTimestampToApply));

            fromTimestampToApply -= (86400000 / 4);
        }
    }

    private string CreateMockedDataFor_AddMessageToChatWindowPayload(string userId, string messageBody, long fromTimestamp)
    {
        var fakeMessage = new ChatMessage()
        {
            body = messageBody,
            sender = userId,
            recipient = UserProfile.GetOwnUserProfile().userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong)fromTimestamp
        };

        return JsonUtility.ToJson(fakeMessage);
    }

    private void CreateFakeUsersInCatalog()
    {
        if (UserProfileController.i == null)
            return;

        for (int i = 0; i < 130; i++)
        {
            var model = new UserProfileModel()
            {
                userId = $"fakeuser{i + 1}",
                name = $"Fake User {i + 1}",
                snapshots = new UserProfileModel.Snapshots
                {
                    face256 = $"https://picsum.photos/seed/{i + 1}/256"
                }
            };

            UserProfileController.i.AddUserProfileToCatalog(model);
        }
    }
}