using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class TermsOfServiceHUD_Should : IntegrationTestSuite_Legacy
{
    protected override bool justSceneSetUp => true;

    private TermsOfServiceHUDController controller;
    private TermsOfServiceHUDView view;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = new TermsOfServiceHUDController();
        view = controller.view;
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void SetTheViewVisibleWhenModelNotNull()
    {
        view.content.SetActive(false);

        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model());

        Assert.IsTrue(view.content.activeSelf);
    }

    [Test]
    public void SetTheViewVisibleWhenModelNull()
    {
        view.content.SetActive(true);

        controller.ShowTermsOfService(null);

        Assert.IsFalse(view.content.activeSelf);
    }

    [Test]
    public void HideViewOnAgreed()
    {
        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model());

        view.agreedButton.onClick.Invoke();

        Assert.IsFalse(view.content.activeSelf);
    }

    [Test]
    public void HideViewOnDeclined()
    {
        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model());

        view.declinedButton.onClick.Invoke();

        Assert.IsFalse(view.content.activeSelf);
    }

    [Test]
    public void ShowAdultWarningProperly()
    {
        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model() {adultContent = true});

        Assert.IsTrue(view.adultContent.activeSelf);
    }

    [Test]
    public void HideAdultWarningProperly()
    {
        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model() {adultContent = false});

        Assert.IsFalse(view.adultContent.activeSelf);
    }

    [Test]
    public void ShowGamlingWarningProperly()
    {
        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model() {gamblingContent = true});

        Assert.IsTrue(view.gamblingContent.activeSelf);
    }

    [Test]
    public void HideGamlingWarningProperly()
    {
        controller.ShowTermsOfService(new TermsOfServiceHUDController.Model() {gamblingContent = false});

        Assert.IsFalse(view.gamblingContent.activeSelf);
    }
}