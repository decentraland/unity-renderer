using DCL;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerNameShould : MonoBehaviour
{
    private PlayerName playerName;

    [SetUp]
    public void SetUp()
    {
        playerName = Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>();
    }

    [TearDown]
    public void TearDown()
    {
        Destroy(playerName.gameObject);
        DataStore.Clear();
    }

    [Test]
    public void BeInitializedProperly()
    {
        Assert.AreEqual(PlayerName.DEFAULT_CANVAS_SORTING_ORDER, playerName.canvas.sortingOrder);
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

    [UnityTest]
    public IEnumerator SetNameCase1()
    {
        yield return SetNameCorrectly("Short");
    }

    [UnityTest]
    public IEnumerator SetNameCase2()
    {
        yield return SetNameCorrectly("VeryLongName");
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

    [UnityTest]
    public IEnumerator ApplyProfanityFilteringToOffensiveNamesCase1()
    {
        yield return ApplyProfanityFilteringToOffensiveNames("shitholename", "****holename");
    }

    [UnityTest]
    public IEnumerator ApplyProfanityFilteringToOffensiveNamesCase2()
    {
        yield return ApplyProfanityFilteringToOffensiveNames("fuckfaceboob", "****face****");
    }

    [UnityTest]
    public IEnumerator SetGuestColor()
    {
        playerName.SetName("hey#83df", false, true);

        // TODO: we should refactor to be able to mock ProfanityFilterSharedInstances.regexFilter
        // internally, executes a UniTask which may take a frame to solve the name filtering making this a flaky test
        yield return null;

        Assert.AreEqual("<color=#A09BA8>hey</color><color=#716B7C>#83df</color>", playerName.nameText.text);
    }

    [UnityTest]
    public IEnumerator SetWeb3Color()
    {
        playerName.SetName("hey#83df", false, false);

        // TODO: we should refactor to be able to mock ProfanityFilterSharedInstances.regexFilter
        // internally, executes a UniTask which may take a frame to solve the name filtering making this a flaky test
        // we could mock the filtering like: UniTask.FromResult("name")
        yield return null;

        Assert.AreEqual("<color=#CFCDD4>hey</color><color=#A09BA8>#83df</color>", playerName.nameText.text);
    }

    [UnityTest]
    public IEnumerator SetClaimedColor()
    {
        playerName.SetName("hey", true, false);

        // TODO: we should refactor to be able to mock ProfanityFilterSharedInstances.regexFilter
        // internally, executes a UniTask which may take a frame to solve the name filtering making this a flaky test
        // we could mock the filtering like: UniTask.FromResult("name")
        yield return null;

        Assert.AreEqual("<color=#FFFFFF>hey</color>", playerName.nameText.text);
    }

    private IEnumerator SetNameCorrectly(string name)
    {
        playerName.SetName(name, false, false);

        // TODO: we should refactor to be able to mock ProfanityFilterSharedInstances.regexFilter
        // internally, executes a UniTask which may take a frame to solve the name filtering making this a flaky test
        // we could mock the filtering like: UniTask.FromResult("name")
        yield return null;

        Assert.AreEqual($"<color=#CFCDD4>{name}</color>", playerName.nameText.text);
        Assert.AreEqual(new Vector2(playerName.nameText.GetPreferredValues().x + PlayerName.BACKGROUND_EXTRA_WIDTH, PlayerName.BACKGROUND_HEIGHT), playerName.background.rectTransform.sizeDelta);
    }

    private IEnumerator ApplyProfanityFilteringToOffensiveNames(string originalName, string displayedName)
    {
        DataStore.i.settings.profanityChatFilteringEnabled.Set(true);
        playerName.SetName(originalName, false, false);

        // TODO: we should refactor to be able to mock ProfanityFilterSharedInstances.regexFilter
        // internally, executes a UniTask which may take a frame to solve the name filtering making this a flaky test
        // we could mock the filtering like: UniTask.FromResult("name")
        yield return null;

        Assert.AreEqual($"<color=#CFCDD4>{displayedName}</color>", playerName.nameText.text);
    }
}
