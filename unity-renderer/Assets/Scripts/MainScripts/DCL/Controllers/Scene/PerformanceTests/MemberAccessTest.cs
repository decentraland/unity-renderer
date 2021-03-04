using DCL;
using MessagingBusTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Unity.PerformanceTesting;
using UnityEngine;

namespace MemberAccessTest
{
    public class MainMemberAccessTest
    {

        private class TestEnvironment {
            public static readonly TestEnvironment i = new TestEnvironment();

            public readonly object someObject;

            private TestEnvironment() {
                someObject = "Some Value";
            }
        }

        [Test, Performance]
        public void AccessLocalVariable()
        {
            object localObject = TestEnvironment.i.someObject;

            Measure.Method(() =>
            {
                for (var i = 0; i < 1000000; i++)
                {
                    localObject.GetType();
                }
            })
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void AccessEnvironmentVariable()
        {
            Measure.Method(() =>
            {
                for (var i = 0; i < 1000000; i++)
                {
                    TestEnvironment.i.someObject.GetType();
                }
            })
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }
    }
}