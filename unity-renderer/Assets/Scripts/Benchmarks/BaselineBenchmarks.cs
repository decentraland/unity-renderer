using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

public class BaselineBenchmarks
{
    [Test, Performance]
    [Explicit]
    [Category("Benchmark")]
    public void BaseLine()
    {
        Measure.Method(() =>
               {
                   for (int i = 0; i < 1000; i++)
                   {
                       float sqrt = Mathf.Sqrt(i);
                       float log = Mathf.Log(sqrt);
                       float pow = Mathf.Pow(log, i);
                   }
               })
               .WarmupCount(3)
               .MeasurementCount(10)
               .IterationsPerMeasurement(10)
               .GC()
               .Run();
    }
}