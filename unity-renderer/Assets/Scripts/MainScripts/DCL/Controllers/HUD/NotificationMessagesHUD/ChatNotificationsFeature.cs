using DCL.ProfanityFiltering;
using DCL.SettingsCommon;
using DCL.Social.Chat;
using DCL.Social.Friends;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    /// <summary>
    /// Plugin feature that initialize the chat notifications feature.
    /// </summary>
    public class ChatNotificationsFeature : IPlugin
    {
        private readonly ChatNotificationController chatNotificationController;

        public ChatNotificationsFeature()
        {
            chatNotificationController = CreateController();
        }

        private ChatNotificationController CreateController()
        {
            ServiceLocator serviceLocator = Environment.i.serviceLocator;

            return new ChatNotificationController(DataStore.i,
                    GameObject.Instantiate(Resources.Load<MainChatNotificationsComponentView>("SocialBarV1/ChatNotificationHUD")),

                GameObject.Instantiate(Resources.Load<TopNotificationComponentView>("SocialBarV1/TopNotificationHUD")),
                serviceLocator.Get<IChatController>(),
                serviceLocator.Get<IFriendsController>(),
                new UserProfileWebInterfaceBridge(),
                serviceLocator.Get<IProfanityFilter>(),
                Settings.i.audioSettings);
        }

        public void Dispose()
        {
            chatNotificationController.Dispose();
        }
    }
}
