using NUnit.Framework;
using System.Collections;
using DCL;
using UnityEngine;
using UnityEngine.TestTools;
using SocialFeaturesAnalytics;
using NSubstitute;

public class UsersAroundListHUDShould : IntegrationTestSuite_Legacy
{
    private UsersAroundListHUDController controller;
    private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = new UsersAroundListHUDController(Substitute.For<ISocialAnalytics>());
    }

    protected override IEnumerator TearDown()
    {
        DataStore.Clear();
        controller.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator CreateView()
    {
        Assert.NotNull(controller.usersListView);
        yield break;
    }

    [UnityTest]
    public IEnumerator HideAndShowWithUI()
    {
        CanvasGroup canvasGroup = (controller.usersListView as UsersAroundListHUDListView).GetComponent<CanvasGroup>();
        Assert.NotNull(canvasGroup, "CanvasGroup is null");

        CommonScriptableObjects.allUIHidden.Set(true);
        Assert.IsTrue(canvasGroup.alpha == 0, "CanvasGroup alpha != 0, should be hidden");
        Assert.IsFalse(canvasGroup.interactable, "CanvasGroup is interactable, should be hidden");

        CommonScriptableObjects.allUIHidden.Set(false);
        Assert.IsTrue(canvasGroup.alpha == 1, "CanvasGroup alpha != 1, should be visible");
        Assert.IsTrue(canvasGroup.interactable, "CanvasGroup is not interactable, should be visible");
        yield break;
    }

    [UnityTest]
    public IEnumerator AddAndRemovePlayersCorrectly()
    {
        UsersAroundListHUDListView listView = controller.usersListView as UsersAroundListHUDListView;

        string[] users = new string[] { "user1", "user2", "user3" };

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            name = users[0],
            userId = users[0]
        });
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = users[1],
            name = users[1]
        });
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            name = users[2],
            userId = users[2]
        });

        otherPlayers.Add(users[0], new Player { id = users[0], name = users[0], worldPosition = Vector3.zero,  playerName = GameObject.Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>() });
        otherPlayers.Add(users[1], new Player { id = users[1], name = users[1], worldPosition = Vector3.zero,  playerName = GameObject.Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>() });

        Assert.IsTrue(GetVisibleChildren(listView.contentPlayers) == 2, "listView.content.childCount != 2");
        Assert.IsTrue(listView.availableElements.Count == 0, "listView.availableElements.Count != 0");

        otherPlayers.Remove(users[1]);
        Assert.IsTrue(GetVisibleChildren(listView.contentPlayers) == 1, "listView.content.childCount != 1");
        Assert.IsTrue(listView.availableElements.Count == 1, "listView.availableElements.Count != 1");

        otherPlayers.Remove(users[0]);
        Assert.IsTrue(GetVisibleChildren(listView.contentPlayers) == 0, "listView.content.childCount != 0");
        Assert.IsTrue(listView.availableElements.Count == 2, "listView.availableElements.Count != 2");

        otherPlayers.Add(users[2], new Player { id = users[2], name = users[2], worldPosition = Vector3.zero,  playerName = GameObject.Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>() });
        Assert.IsTrue(GetVisibleChildren(listView.contentPlayers) == 1, "listView.content.childCount != 1");
        Assert.IsTrue(listView.availableElements.Count == 1, "listView.availableElements.Count != 1");

        yield break;
    }

    int GetVisibleChildren(Transform parent) { return parent.GetComponentsInChildren<UsersAroundListHUDListElementView>(false).Length; }
}