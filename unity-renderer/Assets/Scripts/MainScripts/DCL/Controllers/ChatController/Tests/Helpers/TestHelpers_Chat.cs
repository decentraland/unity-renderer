using DCL.Interface;
using UnityEngine;

public static class TestHelpers_Chat
{
    private static void FakePrivateChatMessage(UserProfileController userProfileController, IChatController controller, string sender, string recipient, string message)
    {
        if (!UserProfileController.userProfilesCatalog.Get(recipient))
        {
            var model = new UserProfileModel()
            {
                userId = recipient,
                name = recipient + "-name",
            };

            userProfileController.AddUserProfileToCatalog(model);
        }

        if (!UserProfileController.userProfilesCatalog.Get(sender))
        {
            var model = new UserProfileModel()
            {
                userId = sender,
                name = sender + "-name",
            };

            userProfileController.AddUserProfileToCatalog(model);
        }

        var msg = new ChatMessage()
        {
            body = message,
            sender = sender,
            recipient = recipient,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = 1000
        };

        controller.AddMessageToChatWindow(JsonUtility.ToJson(msg));
    }

    public static void FakePrivateChatMessageTo(UserProfileController userProfileController, IChatController controller, string recipientUserId, string message)
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();
        FakePrivateChatMessage(userProfileController, controller, ownProfile.userId, recipientUserId, message);
    }

    public static void FakePrivateChatMessageFrom(UserProfileController userProfileController, IChatController controller, string senderUserId, string message)
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();
        FakePrivateChatMessage(userProfileController, controller, senderUserId, ownProfile.userId ?? string.Empty, message);
    }
}