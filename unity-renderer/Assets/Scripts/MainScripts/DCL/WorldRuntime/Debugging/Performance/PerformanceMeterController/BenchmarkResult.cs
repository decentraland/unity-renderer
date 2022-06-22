using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL
{
    [Serializable]
    public class BenchmarkResult
    {
        // metrics based on this // https://github.com/bestiejs/benchmark.js/blob/master/benchmark.js#L1912
        private const double T_TABLE_INFINITY = 1.96;
        private static readonly IReadOnlyDictionary<int, double> tTable = new Dictionary<int, double>()
        {
            { 01, 12.706 }, { 02, 4.303 }, { 03, 3.182 },
            { 04, 02.776 }, { 05, 2.571 }, { 06, 2.447 },
            { 07, 02.365 }, { 08, 2.306 }, { 09, 2.262 },
            { 10, 02.228 }, { 11, 2.201 }, { 12, 2.179 },
            { 13, 02.160 }, { 14, 2.145 }, { 15, 2.131 },
            { 16, 02.120 }, { 17, 2.110 }, { 18, 2.101 },
            { 19, 02.093 }, { 20, 2.086 }, { 21, 2.080 },
            { 22, 02.074 }, { 23, 2.069 }, { 24, 2.064 },
            { 25, 02.060 }, { 26, 2.056 }, { 27, 2.052 },
            { 28, 02.048 }, { 29, 2.045 }, { 30, 2.042 }
        };

        public double min;
        public double max;
        public double moe; // Margin of error
        public double rme = 0; // Relative Margin of Error
        public double sem; // Mean standard error
        public double stdDev;
        public double mean;
        public double variance;
        private double[] samples;

        public BenchmarkResult(double[] samples)
        {
            SetSamples(samples);
        }

        public void SetSamples(double[] samples)
        {
            this.samples = samples;
            int sampleSize = samples.Length;

            min = samples.Min();
            max = samples.Max();
            mean = samples.Average();
            variance = Variance(samples);
            stdDev = StdDev(variance);
            sem = stdDev / Math.Sqrt(sampleSize);
            // Compute the degrees of freedom.
            int df = sampleSize - 1;
            // Compute the critical value.
            int tTableIndex = Math.Max(df, 1); //Minimum index is 1;
            if (!tTable.TryGetValue(tTableIndex, out double critical))
                critical = T_TABLE_INFINITY; // we take the infinity 

            moe = sem * critical;
            if (mean != 0)
                rme = (moe / mean) * 100;
        }

        private static double Variance(double[] samples)
        {
            if (samples.Length <= 1)
                return 0.0;

            double avg = samples.Average();
            double variance = samples.Sum(value => (value - avg) * (value - avg));
            return variance / samples.Length;
        }

        private static double StdDev(double variance) { return Math.Sqrt(variance); }
    }
}