using DCL;
using DCL.Browser;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Social.Chat;
using DCL.ProfanityFiltering;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using Analytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskbarHUDShould : IntegrationTestSuite_Legacy
{
    private TaskbarHUDController controller;
    private TaskbarHUDView view;
    private IFriendsController friendsController;
    private IChatController chatController;
    private PrivateChatWindowController privateChatController;
    private FriendsHUDController friendsHudController;
    private WorldChatWindowController worldChatWindowController;
    private ISocialAnalytics socialAnalytics;
    private IUserProfileBridge userProfileBridge;
    private SearchChannelsWindowController searchChannelsWindowController;
    private ChatChannelHUDController channelHUDController;
    private PublicChatWindowController publicChatWindowController;

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateMouseCatcher());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        friendsController = Substitute.For<IFriendsController>();
        chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedChannel("nearby").Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));

        userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel { name = "myself", userId = "myUserId" });
        userProfileBridge.GetOwn().Returns(ownProfile);

        controller = new TaskbarHUDController(chatController, Substitute.For<IFriendsController>(), Substitute.For<ISupportAnalytics>());
        controller.Initialize(null);
        view = controller.view;

        socialAnalytics = Substitute.For<ISocialAnalytics>();

        Assert.IsTrue(view != null, "Taskbar view is null?");
        Assert.IsTrue(CommonScriptableObjects.isTaskbarHUDInitialized, "Taskbar controller is not initialized?");
    }

    protected override IEnumerator TearDown()
    {
        yield return null;

        privateChatController?.Dispose();
        worldChatWindowController?.Dispose();
        friendsHudController?.Dispose();
        channelHUDController?.Dispose();
        searchChannelsWindowController?.Dispose();
        publicChatWindowController?.Dispose();
        controller.Dispose();

        yield return base.TearDown();
    }

    [Test]
    public void AddWorldChatWindowProperly()
    {
        worldChatWindowController = GivenWorldChatWindowController();
        controller.AddWorldChatWindow(worldChatWindowController);

        Assert.IsTrue(worldChatWindowController.View.Transform.parent == view.leftWindowContainer,
            "Chat window isn't inside taskbar window container!");
    }

    [Test]
    public void AddFriendWindowProperly()
    {
        friendsHudController = GivenFriendsHUDController();
        controller.AddFriendsWindow(friendsHudController);

        Assert.IsTrue(friendsHudController.View.Transform.parent == view.leftWindowContainer,
            "Friends window isn't inside taskbar window container!");
    }

    [Test]
    public void ToggleWindowsProperly()
    {
        privateChatController = GivenPrivateChatWindowController();
        controller.AddPrivateChatWindow(privateChatController);

        worldChatWindowController = GivenWorldChatWindowController();
        controller.AddWorldChatWindow(worldChatWindowController);

        publicChatWindowController = GivenPublicChatWindowController();
        controller.AddPublicChatChannel(publicChatWindowController);

        friendsHudController = GivenFriendsHUDController();
        controller.AddFriendsWindow(friendsHudController);

        searchChannelsWindowController = GivenSearchChannelsWindowController();
        controller.AddChannelSearch(searchChannelsWindowController);

        channelHUDController = GivenChatChannelHudController();
        controller.AddChatChannel(channelHUDController);

        Assert.IsFalse(view.chatButton.toggledOn);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends window on and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindow.View.IsActive);
        Assert.IsTrue(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends window off and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindow.View.IsActive);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends on, and then chat button on. Then check if world chat window is hidden and private chat is showing up.
        view.friendsButton.toggleButton.onClick.Invoke();
        view.chatButton.toggleButton.onClick.Invoke();

        Assert.IsTrue(controller.worldChatWindowHud.View.IsActive);
        Assert.IsFalse(controller.publicChatWindow.View.IsActive);
        Assert.IsFalse(controller.friendsHud.View.IsActive());
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
    }

    private PrivateChatWindowController GivenPrivateChatWindowController()
    {
        IPrivateChatComponentView GivenView()
        {
            var viewObj = new GameObject("PrivateChatWindowMock");
            viewObj.AddComponent<RectTransform>();
            var view = Substitute.For<IPrivateChatComponentView>();
            view.When(x => x.Dispose()).Do(x => Object.Destroy(viewObj));
            view.When(x => x.Hide()).Do(x => viewObj.SetActive(false));
            view.When(x => x.Show()).Do(x => viewObj.SetActive(true));
            view.IsActive.Returns(x=> viewObj.activeSelf);
            view.Transform.Returns((RectTransform)viewObj.transform);
            return view;
        }

        var controller = new PrivateChatWindowController(
            new DataStore(),
            userProfileBridge,
            chatController,
            friendsController,
            socialAnalytics,
            Substitute.For<IMouseCatcher>(),
            Substitute.For<IChatMentionSuggestionProvider>(),
            Substitute.For<IClipboard>());

        controller.Initialize(GivenView());

        return controller;
    }

    private WorldChatWindowController GivenWorldChatWindowController()
    {
        IWorldChatWindowView GivenView()
        {
            var viewObj = new GameObject("WorldChatWindowViewMock");
            viewObj.AddComponent<RectTransform>();
            var view = Substitute.For<IWorldChatWindowView>();
            view.When(x => x.Dispose()).Do(x => Object.Destroy(viewObj));
            view.When(x => x.Hide()).Do(x => viewObj.SetActive(false));
            view.When(x => x.Show()).Do(x => viewObj.SetActive(true));
            view.IsActive.Returns(x=> viewObj.activeSelf);
            view.Transform.Returns((RectTransform)viewObj.transform);
            return view;
        }

        var controller = new WorldChatWindowController(
            userProfileBridge,
            Substitute.For<IFriendsController>(),
            chatController,
            new DataStore(),
            Substitute.For<IMouseCatcher>(),
            Substitute.For<ISocialAnalytics>(),
            Substitute.For<IChannelsFeatureFlagService>(),
            Substitute.For<IBrowserBridge>(),
            CommonScriptableObjects.rendererState,
            new DataStore_Mentions(),
            Substitute.For<IClipboard>());

        controller.Initialize(GivenView());

        return controller;
    }

    private PublicChatWindowController GivenPublicChatWindowController()
    {
        IPublicChatWindowView GivenView()
        {
            var viewObj = new GameObject("PublicChatChannelWindowMock");
            viewObj.AddComponent<RectTransform>();
            var view = Substitute.For<IPublicChatWindowView>();
            view.When(x => x.Dispose()).Do(x => Object.Destroy(viewObj));
            view.When(x => x.Hide()).Do(x => viewObj.SetActive(false));
            view.When(x => x.Show()).Do(x => viewObj.SetActive(true));
            view.IsActive.Returns(x=> viewObj.activeSelf);
            view.Transform.Returns((RectTransform)viewObj.transform);
            return view;
        }

        var controller = new PublicChatWindowController(
            chatController,
            userProfileBridge,
            new DataStore(),
            new RegexProfanityFilter(Substitute.For<IProfanityWordProvider>()),
            Substitute.For<IMouseCatcher>(),
            Substitute.For<IChatMentionSuggestionProvider>(),
            Substitute.For<ISocialAnalytics>(),
            Substitute.For<IClipboard>());

        controller.Initialize(GivenView());

        return controller;
    }

    private FriendsHUDController GivenFriendsHUDController()
    {
        IFriendsHUDComponentView GivenView()
        {
            var viewObj = new GameObject("FriendsHUDWindowMock");
            viewObj.AddComponent<RectTransform>();
            var view = Substitute.For<IFriendsHUDComponentView>();
            view.When(x => x.Dispose()).Do(x => Object.Destroy(viewObj));
            view.When(x => x.Hide()).Do(x => viewObj.SetActive(false));
            view.When(x => x.Show()).Do(x => viewObj.SetActive(true));
            view.IsActive().Returns(x=> viewObj.activeSelf);
            view.Transform.Returns((RectTransform)viewObj.transform);
            return view;
        }

        FriendsHUDController controller = new (new DataStore(), friendsController, userProfileBridge,
            socialAnalytics, chatController,
            Substitute.For<IMouseCatcher>());
        controller.Initialize(GivenView());

        return controller;
    }

    private SearchChannelsWindowController GivenSearchChannelsWindowController()
    {
        ISearchChannelsWindowView GivenView()
        {
            var viewObj = new GameObject("SearchChannelsWindowMock");
            viewObj.AddComponent<RectTransform>();
            var view = Substitute.For<ISearchChannelsWindowView>();
            view.When(x => x.Dispose()).Do(x => Object.Destroy(viewObj));
            view.When(x => x.Hide()).Do(x => viewObj.SetActive(false));
            view.When(x => x.Show()).Do(x => viewObj.SetActive(true));
            view.IsActive.Returns(x=> viewObj.activeSelf);
            view.Transform.Returns((RectTransform)viewObj.transform);
            return view;
        }

        var controller = new SearchChannelsWindowController(chatController, Substitute.For<IMouseCatcher>(), new DataStore(),
            Substitute.For<ISocialAnalytics>(), Substitute.For<IChannelsFeatureFlagService>());

        controller.Initialize(GivenView());

        return controller;
    }

    private ChatChannelHUDController GivenChatChannelHudController()
    {
        IChatChannelWindowView GivenView()
        {
            var viewObj = new GameObject("PrivateChatWindowMock");
            viewObj.AddComponent<RectTransform>();
            viewObj.SetActive(false);
            var view = Substitute.For<IChatChannelWindowView>();
            view.When(x => x.Dispose()).Do(x => Object.Destroy(viewObj));
            view.When(x => x.Hide()).Do(x => viewObj.SetActive(false));
            view.When(x => x.Show()).Do(x => viewObj.SetActive(true));
            view.ChatHUD.Returns(Substitute.For<IChatHUDComponentView>());
            view.IsActive.Returns(x=> viewObj.activeSelf);
            view.Transform.Returns((RectTransform)viewObj.transform);
            return view;
        }

        var controller = new ChatChannelHUDController(new DataStore(),
            userProfileBridge,
            chatController,
            Substitute.For<IMouseCatcher>(),
            socialAnalytics,
            new RegexProfanityFilter(Substitute.For<IProfanityWordProvider>()),
            Substitute.For<IChatMentionSuggestionProvider>(),
            Substitute.For<IClipboard>());

        controller.Initialize(GivenView());

        return controller;
    }
}
