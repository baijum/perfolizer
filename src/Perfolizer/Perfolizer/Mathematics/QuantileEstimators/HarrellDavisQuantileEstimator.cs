using System;
using Perfolizer.Common;
using Perfolizer.Mathematics.Common;
using Perfolizer.Mathematics.Distributions;

namespace Perfolizer.Mathematics.QuantileEstimators
{
    /// <summary>
    /// <remarks>
    /// Harrell, F.E. and Davis, C.E., 1982. A new distribution-free quantile estimator. Biometrika, 69(3), pp.635-640.
    /// </remarks>
    /// </summary>
    public class HarrellDavisQuantileEstimator : IQuantileEstimator, IQuantileConfidenceIntervalEstimator
    {
        public static readonly HarrellDavisQuantileEstimator Instance = new HarrellDavisQuantileEstimator();

        public bool SupportsWeightedSamples => true;

        public double GetQuantile(Sample sample, double probability)
        {
            return GetMoments(sample, probability, false).C1;
        }

        /// <summary>
        /// Estimates confidence intervals using the Maritz-Jarrett method
        /// </summary>
        /// <returns></returns>
        public ConfidenceIntervalEstimator GetQuantileConfidenceIntervalEstimator(Sample sample, double probability)
        {
            (double c1, double c2) = GetMoments(sample, probability, true);
            double median = c1;
            double standardError = Math.Sqrt(c2 - c1.Sqr());
            return new ConfidenceIntervalEstimator(sample.Count, median, standardError);
        }

        private readonly struct Moments
        {
            public readonly double C1;
            public readonly double C2;

            public Moments(double c1, double c2)
            {
                C1 = c1;
                C2 = c2;
            }

            public void Deconstruct(out double c1, out double c2)
            {
                c1 = C1;
                c2 = C2;
            }
        }

        private static Moments GetMoments(Sample sample, double probability, bool calcSecondMoment)
        {
            Assertion.NotNull(nameof(sample), sample);
            Assertion.InRangeInclusive(nameof(probability), probability, 0, 1);

            int n = sample.Count;
            double a = (n + 1) * probability, b = (n + 1) * (1 - probability);
            var distribution = new BetaDistribution(a, b);

            double c1 = 0;
            double c2 = calcSecondMoment ? 0 : double.NaN;
            double betaCdfRight = 0;
            double currentProbability = 0;
            for (int j = 0; j < sample.Count; j++)
            {
                double betaCdfLeft = betaCdfRight;
                currentProbability += sample.SortedWeights[j] / sample.TotalWeight;
                betaCdfRight = distribution.Cdf(currentProbability);
                double w = betaCdfRight - betaCdfLeft;
                c1 += w * sample.SortedValues[j];
                if (calcSecondMoment)
                    c2 += w * sample.SortedValues[j].Sqr();
            }

            return new Moments(c1, c2);
        }
    }
}