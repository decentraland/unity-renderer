using Cysharp.Threading.Tasks;
using DCL;
using DCL.ProfanityFiltering;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class PlayerNameShould : MonoBehaviour
{
    private PlayerName playerName;
    private IProfanityFilter profanityFilter;

    [SetUp]
    public void SetUp()
    {
        playerName = Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>();
        ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
        Environment.Setup(serviceLocator);
        profanityFilter = serviceLocator.Get<IProfanityFilter>();
        profanityFilter.Filter(Arg.Any<string>()).Returns(info => UniTask.FromResult(info[0].ToString()));
    }

    [Test]
    public void BeInitializedProperly()
    {
        Assert.AreEqual(PlayerName.TARGET_ALPHA_SHOW, playerName.Alpha);
        Assert.AreEqual(playerName.backgroundOriginalColor, playerName.background.color);
        Assert.IsTrue(playerName.gameObject.activeSelf);
        Assert.IsTrue(playerName.canvas.enabled);
    }

    [Test]
    [TestCase(0)]
    [TestCase(0.5f)]
    [TestCase(1)]
    public void ChangeOnlyBackgroundAlphaOnOpacityChange(float newOpacity)
    {
        playerName.OnNamesOpacityChanged(newOpacity, -1);
        Assert.AreEqual(playerName.backgroundOriginalColor.r, playerName.background.color.r);
        Assert.AreEqual(playerName.backgroundOriginalColor.g, playerName.background.color.g);
        Assert.AreEqual(playerName.backgroundOriginalColor.b, playerName.background.color.b);
        Assert.AreEqual(newOpacity, playerName.background.color.a);
    }

    [Test]
    [TestCase(0)]
    [TestCase(0.5f)]
    [TestCase(1)]
    public void ReactToNameOpacityChange(float newOpacity)
    {
        DataStore.i.HUDs.avatarNamesOpacity.Set(newOpacity, true);
        Assert.AreEqual(playerName.backgroundOriginalColor.r, playerName.background.color.r);
        Assert.AreEqual(playerName.backgroundOriginalColor.g, playerName.background.color.g);
        Assert.AreEqual(playerName.backgroundOriginalColor.b, playerName.background.color.b);
        Assert.AreEqual(newOpacity, playerName.background.color.a);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReactToNamesVisibility(bool visible)
    {
        DataStore.i.HUDs.avatarNamesVisible.Set(visible, true);
        Assert.AreEqual(visible, playerName.canvas.enabled);
    }

    [Test]
    [TestCase("Short")]
    [TestCase("VeryLongName")]
    public void SetNameCorrectly(string name)
    {
        profanityFilter.Filter(Arg.Any<string>()).Returns(info => UniTask.FromResult(info[0].ToString()));

        playerName.SetName(name, false, false);

        Assert.AreEqual($"<color=#CFCDD4>{name}</color>", playerName.nameText.text);
        Assert.AreEqual(new Vector2(playerName.nameText.GetPreferredValues().x + PlayerName.BACKGROUND_EXTRA_WIDTH, PlayerName.BACKGROUND_HEIGHT), playerName.background.rectTransform.sizeDelta);
    }

    [Test]
    public void DisableRenderersIfHidden()
    {
        playerName.Alpha = 0;
        playerName.TargetAlpha = 0;
        playerName.Update(float.MaxValue);

        var canvasRenderers = playerName.canvasRenderers;
        Assert.IsTrue(canvasRenderers != null
                      && canvasRenderers.
                          TrueForAll(r =>
                              r.GetAlpha() < 0.01f));
    }

    [Test]
    public void StepTowardsAlphaTargetProperly_Ascending()
    {
        playerName.Alpha = 0;
        playerName.TargetAlpha = 1;
        playerName.Update(0.01f);

        Assert.AreEqual(PlayerName.ALPHA_TRANSITION_STEP_PER_SECOND * 0.01f, playerName.Alpha);
    }

    [Test]
    public void StepTowardsAlphaTargetProperly_Descending()
    {
        playerName.Alpha = 1;
        playerName.TargetAlpha = 0;
        playerName.Update(0.01f);

        Assert.AreEqual(1f - PlayerName.ALPHA_TRANSITION_STEP_PER_SECOND * 0.01f, playerName.Alpha);
    }

    [Test]
    public void LookAtCameraCorrectly()
    {
        GameObject cameraStub = new GameObject("_CameraStub");
        cameraStub.transform.position = Vector3.one * 2;
        cameraStub.transform.LookAt(new Vector3(0.02f, 0.04f, 0.05f));
        playerName.gameObject.transform.position = Vector3.zero;

        playerName.LookAtCamera(cameraStub.transform.right, cameraStub.transform.eulerAngles);

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, -cameraStub.transform.right); // Fix Y rotation
        rotation.eulerAngles = new Vector3(-cameraStub.transform.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z); // Set X rotation to stay in the horizont

        Assert.AreEqual(rotation.eulerAngles, playerName.transform.localEulerAngles);
        Destroy(cameraStub);
    }

    [TestCase("filteredname", "***teredn***")]
    [TestCase("otherfilteredname", "***erfilte***name")]
    public void ApplyProfanityFilteringToOffensiveNames(string originalName, string displayedName)
    {
        DataStore.i.settings.profanityChatFilteringEnabled.Set(true);
        profanityFilter.Filter(originalName).Returns(UniTask.FromResult(displayedName));

        playerName.SetName(originalName, false, false);

        Assert.AreEqual($"<color=#CFCDD4>{displayedName}</color>", playerName.nameText.text);
    }

    [Test]
    public void SetGuestColor()
    {
        const string NAME = "hey#83df";
        profanityFilter.Filter(NAME).Returns(UniTask.FromResult(NAME));

        playerName.SetName(NAME, false, true);

        Assert.AreEqual("<color=#A09BA8>hey</color><color=#716B7C>#83df</color>", playerName.nameText.text);
    }

    [Test]
    public void SetWeb3Color()
    {
        const string NAME = "hey#83df";
        profanityFilter.Filter(NAME).Returns(UniTask.FromResult(NAME));

        playerName.SetName(NAME, false, false);

        Assert.AreEqual("<color=#CFCDD4>hey</color><color=#A09BA8>#83df</color>", playerName.nameText.text);
    }

    [Test]
    public void SetClaimedColor()
    {
        const string NAME = "hey";
        profanityFilter.Filter(NAME).Returns(UniTask.FromResult(NAME));

        playerName.SetName(NAME, true, false);

        Assert.AreEqual("<color=#FFFFFF>hey</color>", playerName.nameText.text);
    }

    [TearDown]
    public void TearDown()
    {
        Destroy(playerName.gameObject);
        DataStore.Clear();
    }
}
