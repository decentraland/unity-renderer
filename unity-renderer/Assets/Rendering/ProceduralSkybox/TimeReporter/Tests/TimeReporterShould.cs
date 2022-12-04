using DCL;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class TimeReporterShould
    {
        private TimeReporter timeReporter;
        private IDummyEventSubscriber<float, bool> reportSubscriber;

        [SetUp]
        public void SetUp()
        {
            timeReporter = new TimeReporter();
            reportSubscriber = Substitute.For<IDummyEventSubscriber<float, bool>>();
            timeReporter.OnReport += reportSubscriber.React;
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void AlwaysReportWhenUserChangesTime()
        {
            DataStore.i.skyboxConfig.mode.Set(SkyboxMode.HoursFixedByUser);
            timeReporter.ReportTime(111);
            reportSubscriber.Received(1).React(111, true);
            timeReporter.ReportTime(928);
            reportSubscriber.Received(1).React(928, true);
        }

        [Test]
        public void DoNotReportMoreThanOnceWhenDynamicSkyboxIsEnabled()
        {
            DataStore.i.skyboxConfig.mode.Set(SkyboxMode.Dynamic);
            timeReporter.ReportTime(111);
            reportSubscriber.Received(1).React(111, false);
            reportSubscriber.ClearReceivedCalls();
            timeReporter.ReportTime(928);
            reportSubscriber.DidNotReceive().React(Arg.Any<float>(), Arg.Any<bool>());
        }

        [Test]
        public void ReportCorrectlyOnTogglingDynamicSkybox()
        {
            DataStore.i.skyboxConfig.mode.Set(SkyboxMode.Dynamic);
            timeReporter.ReportTime(111);
            reportSubscriber.Received(1).React(111, false);

            DataStore.i.skyboxConfig.mode.Set(SkyboxMode.HoursFixedByUser);
            timeReporter.ReportTime(928);
            reportSubscriber.Received(1).React(928, true);

            DataStore.i.skyboxConfig.mode.Set(SkyboxMode.Dynamic);
            timeReporter.ReportTime(632);
            reportSubscriber.Received(1).React(632, false);
        }
    }
}
