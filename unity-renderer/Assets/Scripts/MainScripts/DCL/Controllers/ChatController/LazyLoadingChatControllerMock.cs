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

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        // TODO:
        // 1. Prepare a set of fake data
        // 2. Delay
        // 3. Simulate the kernel response (call to the corresponding controller method that manage the response)
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