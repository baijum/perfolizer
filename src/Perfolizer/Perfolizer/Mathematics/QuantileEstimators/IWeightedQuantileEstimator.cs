using System.Collections.Generic;
using JetBrains.Annotations;
using Perfolizer.Collections;

namespace Perfolizer.Mathematics.QuantileEstimators
{
    public interface IWeightedQuantileEstimator : IQuantileEstimator
    {
        double GetWeightedQuantile([NotNull] ISortedReadOnlyList<double> data, [NotNull] IReadOnlyList<double> weights, double probability);
    }
}