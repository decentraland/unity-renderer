using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.SettingsCommon;
using DCL.Social.Chat;
using DCL.Social.Friends;
using System.Threading;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    /// <summary>
    /// Plugin feature that initialize the chat notifications feature.
    /// </summary>
    public class ChatNotificationsFeature : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private ChatNotificationController chatNotificationController;

        public ChatNotificationsFeature()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            ServiceLocator serviceLocator = Environment.i.serviceLocator;

            var chatView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<MainChatNotificationsComponentView>("ChatNotificationHUD", cancellationToken: ct);

            var notificationView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                         .Instantiate<TopNotificationComponentView>("TopNotificationHUD", cancellationToken: ct);

            chatNotificationController = new ChatNotificationController(DataStore.i,
                chatView,
                notificationView,
                serviceLocator.Get<IChatController>(),
                serviceLocator.Get<IFriendsController>(),
                new UserProfileWebInterfaceBridge(),
                serviceLocator.Get<IProfanityFilter>(),
                Settings.i.audioSettings);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            chatNotificationController.Dispose();
        }
    }
}
