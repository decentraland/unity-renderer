using DCL.World.PortableExperiences;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupControllerShould
    {
        private ExperiencesConfirmationPopupController controller;
        private IExperiencesConfirmationPopupView view;
        private DataStore dataStore;
        private IConfirmedExperiencesRepository confirmedExperiencesRepository;
        private IUserProfileBridge userProfileBridge;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IExperiencesConfirmationPopupView>();

            dataStore = new DataStore();

            confirmedExperiencesRepository = Substitute.For<IConfirmedExperiencesRepository>();

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            UserProfile ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = "ownId",
            });

            userProfileBridge.GetOwn().Returns(ownUserProfile);

            controller = new ExperiencesConfirmationPopupController(
                view,
                dataStore,
                confirmedExperiencesRepository,
                userProfileBridge);

            view.ClearReceivedCalls();
        }

        [Test]
        public void Open()
        {
            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                    Permissions = Array.Empty<string>(),
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1).Show();
        }

        [Test]
        public void ShowIconUrl()
        {
            const string ICON_URL = "http://iconurl.com";

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                    Permissions = Array.Empty<string>(),
                    IconUrl = ICON_URL,
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1)
                .SetModel(Arg.Is<ExperiencesConfirmationViewModel>(e =>
                     e.IconUrl == ICON_URL));
        }

        [Test]
        public void ShowName()
        {
            const string EXPERIENCE_NAME = "PX Name";

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                    Permissions = Array.Empty<string>(),
                    ExperienceName = EXPERIENCE_NAME,
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1)
                .SetModel(Arg.Is<ExperiencesConfirmationViewModel>(e =>
                     e.Name == EXPERIENCE_NAME));
        }

        [Test]
        public void ShowDescription()
        {
            const string DESCRIPTION = "Desc";

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                    Permissions = Array.Empty<string>(),
                    Description = DESCRIPTION,
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1)
                .SetModel(Arg.Is<ExperiencesConfirmationViewModel>(e =>
                     e.Description == DESCRIPTION));
        }

        [Test]
        public void ShowPermissions()
        {
            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                    Permissions = new[]
                    {
                        "USE_FETCH",
                        "USE_WEBSOCKET",
                        "OPEN_EXTERNAL_LINK",
                        "USE_WEB3_API",
                        "ALLOW_TO_TRIGGER_AVATAR_EMOTE",
                        "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE",
                        "ALLOW_MEDIA_HOSTNAMES",
                    },
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1)
                .SetModel(Arg.Is<ExperiencesConfirmationViewModel>(e =>
                     e.Permissions[0] == "Communicate with 3rd party servers."
                     && e.Permissions[1] == "Exchange data with other servers."
                     && e.Permissions[2] == "Open external links."
                     && e.Permissions[3] == "Interact with your wallet."
                     && e.Permissions[4] == "Trigger emotes."
                     && e.Permissions[5] == "Move your position."
                     && e.Permissions[6] == "Play media content (video, audio, etc)."));
        }

        [Test]
        public void SmartWearableWhenPortableExperienceIsInTheWearableList()
        {
            const string SMART_WEARABLE_ID = "smartWearableId";

            UserProfile ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = "ownId",
                avatar = new AvatarModel
                {
                    wearables = new List<string> { SMART_WEARABLE_ID }
                },
            });

            userProfileBridge.GetOwn().Returns(ownUserProfile);

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = SMART_WEARABLE_ID,
                    Permissions = Array.Empty<string>(),
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1).SetModel(Arg.Is<ExperiencesConfirmationViewModel>(e =>
                e.IsSmartWearable));
        }

        [Test]
        public void IsNotSmartWearableWhenIsNotInTheWearableList()
        {
            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                    Permissions = Array.Empty<string>(),
                },
                OnAcceptCallback = () => { },
                OnRejectCallback = () => { },
            });

            view.Received(1).SetModel(Arg.Is<ExperiencesConfirmationViewModel>(e =>
                !e.IsSmartWearable));
        }

        [Test]
        public void Accept()
        {
            bool acceptCalled = false;
            bool rejectCalled = false;

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                },
                OnAcceptCallback = () =>
                {
                    acceptCalled = true;
                },
                OnRejectCallback = () =>
                {
                    rejectCalled = true;
                },
            });

            view.OnKeepShowing += Raise.Event<Action>();
            view.OnAccepted += Raise.Event<Action>();

            Assert.IsTrue(acceptCalled);
            Assert.IsFalse(rejectCalled);

            confirmedExperiencesRepository.DidNotReceiveWithAnyArgs().Set(default, default);
        }

        [Test]
        public void AcceptAndStoreItAsConfirmed()
        {
            bool acceptCalled = false;
            bool rejectCalled = false;

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                },
                OnAcceptCallback = () =>
                {
                    acceptCalled = true;
                },
                OnRejectCallback = () =>
                {
                    rejectCalled = true;
                },
            });

            view.OnDontShowAnymore += Raise.Event<Action>();
            view.OnAccepted += Raise.Event<Action>();

            Assert.IsTrue(acceptCalled);
            Assert.IsFalse(rejectCalled);

            confirmedExperiencesRepository.Received(1).Set("pxId", true);
        }

        [Test]
        public void Reject()
        {
            bool acceptCalled = false;
            bool rejectCalled = false;

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                },
                OnAcceptCallback = () =>
                {
                    acceptCalled = true;
                },
                OnRejectCallback = () =>
                {
                    rejectCalled = true;
                },
            });

            view.OnKeepShowing += Raise.Event<Action>();
            view.OnRejected += Raise.Event<Action>();

            Assert.IsFalse(acceptCalled);
            Assert.IsTrue(rejectCalled);

            confirmedExperiencesRepository.DidNotReceiveWithAnyArgs().Set(default, default);
        }

        [Test]
        public void RejectAndStoreItAsConfirmed()
        {
            bool acceptCalled = false;
            bool rejectCalled = false;

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                },
                OnAcceptCallback = () =>
                {
                    acceptCalled = true;
                },
                OnRejectCallback = () =>
                {
                    rejectCalled = true;
                },
            });

            view.OnDontShowAnymore += Raise.Event<Action>();
            view.OnRejected += Raise.Event<Action>();

            Assert.IsFalse(acceptCalled);
            Assert.IsTrue(rejectCalled);

            confirmedExperiencesRepository.Received(1).Set("pxId", false);
        }

        [Test]
        public void Cancel()
        {
            bool acceptCalled = false;
            bool rejectCalled = false;

            dataStore.world.portableExperiencePendingToConfirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    ExperienceId = "pxId",
                },
                OnAcceptCallback = () =>
                {
                    acceptCalled = true;
                },
                OnRejectCallback = () =>
                {
                    rejectCalled = true;
                },
            });

            view.OnDontShowAnymore += Raise.Event<Action>();
            view.OnCancelled += Raise.Event<Action>();

            Assert.IsFalse(acceptCalled);
            Assert.IsTrue(rejectCalled);

            confirmedExperiencesRepository.DidNotReceiveWithAnyArgs().Set(default, default);
        }
    }
}
