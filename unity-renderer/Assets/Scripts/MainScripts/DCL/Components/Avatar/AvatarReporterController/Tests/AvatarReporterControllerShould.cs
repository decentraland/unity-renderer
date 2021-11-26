using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class AvatarReporterControllerShould
{
    private IAvatarReporterController reporterController;

    [SetUp]
    public void SetUp()
    {
        reporterController = Substitute.ForPartsOf<AvatarReporterController>();
        reporterController.reporter = Substitute.For<IReporter>();
    }

    [Test]
    public void ReportAvatarPositionWhenIsSetup()
    {
        reporterController.SetUp(AvatarReporterController.AVATAR_GLOBAL_SCENE, "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarPosition("0", "1", Vector3.one);
    }

    [Test]
    public void DontReportAvatarPositionWhenIsNotSetup()
    {
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarPosition("0", "1", Vector3.one);
    }

    [Test]
    public void DontReportAvatarPositionWhenIsNotFromAvatarGlobalScene()
    {
        reporterController.SetUp("Temptation", "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarPosition("0", "1", Vector3.one);
    }

    [Test]
    public void DontReportAvatarPositionAfterAvatarRemoved()
    {
        reporterController.SetUp(AvatarReporterController.AVATAR_GLOBAL_SCENE, "0", "1");
        reporterController.ReportAvatarRemoved();
        reporterController.reporter.Received(1).ReportAvatarRemoved("0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarPosition("0", "1", Vector3.one);
    }

    [Test]
    public void DontReportAvatarPositionIfAvatarDidntMove()
    {
        reporterController.SetUp(AvatarReporterController.AVATAR_GLOBAL_SCENE, "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarPosition("0", "1", Vector3.one);
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarPosition("0", "1", Vector3.one);
    }
}