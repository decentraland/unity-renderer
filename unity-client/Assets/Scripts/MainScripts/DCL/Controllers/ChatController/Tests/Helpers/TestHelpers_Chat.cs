using DCL.Interface;
using UnityEngine;

public static class TestHelpers_Chat
{
    private static void FakePrivateChatMessage(IChatController controller, string sender, string recipient, string message)
    {
        if (!UserProfileController.userProfilesCatalog.Get(recipient))
        {
            var model = new UserProfileModel()
            {
                userId = recipient,
                name = recipient + "-name",
            };

            UserProfileController.i.AddUserProfileToCatalog(model);
        }

        if (!UserProfileController.userProfilesCatalog.Get(sender))
        {
            var model = new UserProfileModel()
            {
                userId = sender,
                name = sender + "-name",
            };

            UserProfileController.i.AddUserProfileToCatalog(model);
        }

        var msg = new ChatMessage()
        {
            body = message,
            sender = sender,
            recipient = recipient,
            messageType = ChatMessage.Type.PRIVATE
        };

        controller.AddMessageToChatWindow(JsonUtility.ToJson(msg));
    }

    public static void FakePrivateChatMessageTo(IChatController controller, string recipientUserId, string message)
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();
        FakePrivateChatMessage(controller, ownProfile.userId, recipientUserId, message);
    }

    public static void FakePrivateChatMessageFrom(IChatController controller, string senderUserId, string message)
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();
        FakePrivateChatMessage(controller, senderUserId, ownProfile.userId, message);
    }
}
