using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCL.Social.Chat
{
    public partial class ChatController
    {
        [ContextMenu("Fake Public Message")]
        public void FakePublicMessage()
        {
            UserProfile ownProfile = UserProfile.GetOwnUserProfile();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bodyLength = Random.Range(5, 30);
            var bodyText = "";
            for (var i = 0; i < bodyLength; i++)
                bodyText += chars[Random.Range(0, chars.Length)];

            var model = new UserProfileModel
            {
                userId = "test user 1",
                name = "test user 1",
            };

            UserProfileController.i.AddUserProfileToCatalog(model);

            var model2 = new UserProfileModel()
            {
                userId = "test user 2",
                name = "test user 2",
            };

            UserProfileController.i.AddUserProfileToCatalog(model2);

            var msg = new ChatMessage()
            {
                body = bodyText,
                sender = model.userId,
                messageType = ChatMessage.Type.PUBLIC,
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var msg2 = new ChatMessage()
            {
                body = bodyText,
                sender = ownProfile.userId,
                messageType = ChatMessage.Type.PUBLIC,
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            AddMessages(new[] {msg, msg2});
        }

        [ContextMenu("Fake Private Message")]
        public void FakePrivateMessage()
        {
            UserProfile ownProfile = UserProfile.GetOwnUserProfile();

            var model = new UserProfileModel()
            {
                userId = "test user 1",
                name = "test user 1",
            };

            UserProfileController.i.AddUserProfileToCatalog(model);

            var model2 = new UserProfileModel()
            {
                userId = "test user 2",
                name = "test user 2",
            };

            UserProfileController.i.AddUserProfileToCatalog(model2);

            var msg = new ChatMessage()
            {
                body = "test message",
                sender = model.userId,
                recipient = ownProfile.userId,
                messageType = ChatMessage.Type.PRIVATE,
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var msg2 = new ChatMessage()
            {
                body = "test message 2",
                recipient = model2.userId,
                sender = ownProfile.userId,
                messageType = ChatMessage.Type.PRIVATE,
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            AddMessages(new[] {msg, msg2});
        }

        [ContextMenu("Add fake messages to nearby")]
        public async UniTask AddManyFakeMessagesToNearby()
        {
            var users = new List<UserProfileModel>();

            for (var i = 0; i < 10; i++)
            {
                var userId = Guid.NewGuid().ToString();
                var userProfile = new UserProfileModel
                {
                    userId = userId,
                    name = $"TestUser{i}"
                };

                UserProfileController.i.AddUserProfileToCatalog(userProfile);
                users.Add(userProfile);
            }

            for (var i = 0; i < 100; i++)
            {
                var user = users[Random.Range(0, users.Count)];

                var msg = new ChatMessage
                {
                    body = "test message",
                    sender = user.userId,
                    recipient = null,
                    messageType = ChatMessage.Type.PUBLIC,
                    timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    messageId = Guid.NewGuid().ToString(),
                    senderName = user.name
                };

                AddMessages(new[] {msg});
                // for a real scenario, only one message is added by frame
                await UniTask.NextFrame();
            }
        }

        [ContextMenu("Add fake messages to global")]
        public async UniTask AddManyFakeMessagesToUnityChannel()
        {
            var users = new List<UserProfileModel>();

            for (var i = 0; i < 10; i++)
            {
                var userId = Guid.NewGuid().ToString();
                var userProfile = new UserProfileModel
                {
                    userId = userId,
                    name = $"TestUser{i}"
                };

                UserProfileController.i.AddUserProfileToCatalog(userProfile);
                users.Add(userProfile);
            }

            for (var i = 0; i < 100; i++)
            {
                var user = users[Random.Range(0, users.Count)];

                var msg = new ChatMessage
                {
                    body = "test message",
                    sender = user.userId,
                    recipient = GetAllocatedChannelByName("global").ChannelId,
                    messageType = ChatMessage.Type.PUBLIC,
                    timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    messageId = Guid.NewGuid().ToString(),
                    senderName = user.name
                };

                AddMessages(new[] {msg});
                // for a real scenario, only one message is added by frame
                await UniTask.NextFrame();
            }
        }
    }
}
