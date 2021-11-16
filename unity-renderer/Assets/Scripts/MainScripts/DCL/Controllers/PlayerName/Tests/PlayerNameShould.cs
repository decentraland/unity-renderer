using DCL;
using NUnit.Framework;
using UnityEngine;

public class PlayerNameShould : MonoBehaviour
{
    private PlayerName playerName;

    [SetUp]
    public void SetUp() { playerName = Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>(); }

    [Test]
    public void BeInitializedProperly()
    {
        Assert.AreEqual(PlayerName.DEFAULT_CANVAS_SORTING_ORDER, playerName.canvas.sortingOrder);
        Assert.AreEqual(PlayerName.TARGET_ALPHA_SHOW, playerName.alpha);
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
        playerName.SetName(name);
        Assert.AreEqual(name, playerName.nameText.text);
        Assert.AreEqual(new Vector2(playerName.nameText.GetPreferredValues().x + PlayerName.BACKGROUND_EXTRA_WIDTH, PlayerName.BACKGROUND_HEIGHT), playerName.background.rectTransform.sizeDelta);
    }

    [Test]
    public void DisableTheGameObjectIfHidden()
    {
        playerName.alpha = 0;
        playerName.targetAlpha = 0;
        playerName.Update(float.MaxValue);

        Assert.IsFalse(playerName.gameObject.activeSelf);
    }

    [Test]
    public void StepTowardsAlphaTargetProperly_Ascending()
    {
        playerName.alpha = 0;
        playerName.targetAlpha = 1;
        playerName.Update(0.01f);

        Assert.AreEqual(PlayerName.ALPHA_TRANSITION_STEP_PER_SECOND * 0.01f, playerName.alpha);
    }

    [Test]
    public void StepTowardsAlphaTargetProperly_Descending()
    {
        playerName.alpha = 1;
        playerName.targetAlpha = 0;
        playerName.Update(0.01f);

        Assert.AreEqual(1f - PlayerName.ALPHA_TRANSITION_STEP_PER_SECOND * 0.01f, playerName.alpha);
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

    [TestCase("shitholename", "****holename")]
    [TestCase("fuckfaceboob", "****face****")]
    public void ApplyProfanityFilteringToOffensiveNames(string originalName, string displayedName)
    {
        playerName.SetName(originalName);
        Assert.AreEqual(displayedName, playerName.nameText.text);
    }

    [TearDown]
    public void TearDown()
    {
        Destroy(playerName.gameObject);
        DataStore.Clear();
    }
}