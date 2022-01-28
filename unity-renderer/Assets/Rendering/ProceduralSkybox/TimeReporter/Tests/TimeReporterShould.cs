using DCL;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class TimeReporterShould
    {
        private TimeReporter timeReporter;

        [SetUp]
        public void SetUp()
        {
            timeReporter = Substitute.ForPartsOf<TimeReporter>();
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void AlwaysReportWhenUserChangesTime()
        {
            DataStore.i.skyboxConfig.useDynamicSkybox.Set(false);
            timeReporter.ReportTime(111);
            timeReporter.Received(1).DoReport(111, true);
            timeReporter.ReportTime(928);
            timeReporter.Received(1).DoReport(928, true);
        }

        [Test]
        public void DoNotReportMoreThanOnceWhenDynamicSkyboxIsEnabled()
        {
            DataStore.i.skyboxConfig.useDynamicSkybox.Set(true);
            timeReporter.ReportTime(111);
            timeReporter.Received(1).DoReport(111, false);
            timeReporter.ReportTime(928);
            timeReporter.DidNotReceive().DoReport(Arg.Any<float>(), Arg.Any<bool>());
        }

        [Test]
        public void ReportCorrectlyOnTogglingDynamicSkybox()
        {
            DataStore.i.skyboxConfig.useDynamicSkybox.Set(true);
            timeReporter.ReportTime(111);
            timeReporter.Received(1).DoReport(111, false);

            DataStore.i.skyboxConfig.useDynamicSkybox.Set(false);
            timeReporter.ReportTime(928);
            timeReporter.Received(1).DoReport(928, true);

            DataStore.i.skyboxConfig.useDynamicSkybox.Set(true);
            timeReporter.ReportTime(632);
            timeReporter.Received(1).DoReport(632, false);
        }
    }
}