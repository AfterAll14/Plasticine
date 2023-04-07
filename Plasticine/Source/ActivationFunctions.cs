using System;
using real = System.Double;

namespace Plasticine
{
    public static class ActivationFunctions
    {
        public static (real, real) Sigmoid(real[] weightedSums, int valueId)
        {
            real exp = Utils.ExpClamped(weightedSums[valueId]);
            real y = exp / (exp + 1);
            real dydx = y * ((real)1.0 - y);

            return (y, dydx);
        }

        public static (real, real) ReLU(real[] weightedSums, int valueId)
        {
            return weightedSums[valueId] < 0 ? (0, 0) : (weightedSums[valueId], 1);
        }

        public static (real, real) LeakyReLU(real[] weightedSums, int valueId)
        {
            real value = weightedSums[valueId];
            real slope = value < 0 ? (real)0.01 : (real)1;
            return (value * slope, slope);
        }

        public static (real, real) SoftMax(real[] weightedSums, int valueId)
        {
            real maxSum = weightedSums[0];
            for (int i = 1; i < weightedSums.Length; i++)
                if (weightedSums[i] > maxSum)
                    maxSum = weightedSums[i];

            real dividerSum = 0;
            for (int i = 0; i < weightedSums.Length; i++)
                if (i != valueId)
                    dividerSum += Utils.ExpClamped(weightedSums[i] - maxSum);

            real valueExp = Utils.ExpClamped(weightedSums[valueId] - maxSum);

            real result = valueExp / (dividerSum + valueExp);
            real resultDerivative = result * ((real)1.0 - result);

            return (result, resultDerivative);
        }
    }
}