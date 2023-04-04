using NSubstitute;
using NUnit.Framework;
using DCL.Social.Passports;
using DCL;
using System.Threading;

public class PassportHUDViewShould
{
    private PlayerPassportHUDView view;

    [SetUp]
    public void SetUp()
    {
        view = PlayerPassportHUDView.CreateView();
        view.Initialize(Substitute.For<MouseCatcher>());
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    public void Show()
    {
        view.SetVisibility(true);
        Assert.IsTrue(view.gameObject.activeSelf);
    }

    [Test]
    public void Hide()
    {
        view.SetVisibility(false);
        Assert.IsFalse(view.gameObject.activeSelf);
    }

    [Test]
    public void SetPassportPanelVisibilityTrue()
    {
        view.SetVisibility(true);
        view.SetPassportPanelVisibility(true);

        Assert.IsTrue(view.gameObject.activeSelf);
        Assert.IsTrue(view.container.activeSelf);
    }

    [Test]
    public void SetPassportPanelVisibilityFalse()
    {
        view.SetVisibility(true);
        view.SetPassportPanelVisibility(false);

        Assert.IsTrue(view.gameObject.activeSelf);
    }

}
