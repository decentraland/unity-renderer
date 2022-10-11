using DCL;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace AvatarShape_Tests
{
    public class AvatarShapeBenchmarks
    {
        [Test, Performance]
        public void MeasureNullAvatarCheck()
        {
            AvatarMovementController avatarMovementController = null;
            Measure.Method(() =>
            {
                if (avatarMovementController == null)
                    return;
            })
            .WarmupCount(3)
            .MeasurementCount(1000)
            .IterationsPerMeasurement(1000)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void MeasureBoolAvatarCheck()
        {
            bool avatarMovementControllerIsNull = true;
            Measure.Method(() =>
            {
                if (avatarMovementControllerIsNull)
                    return;
            })
            .WarmupCount(3)
            .MeasurementCount(1000)
            .IterationsPerMeasurement(1000)
            .GC()
            .Run();
        }
    }
}