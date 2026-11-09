// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.

using System;

namespace ProbablyStolenMixer
{
    /// <summary>Plans whole metered transfers using the game's integer component allocation.</summary>
    public static class MeteredMixPlanner
    {
        private const double ComparisonSlack = 1e-15;

        public static int ResolutionMicroliters(int moduleCount)
        {
            switch (Math.Max(0, Math.Min(4, moduleCount)))
            {
                case 0: return 10000;
                case 1: return 5000;
                case 2: return 1000;
                case 3: return 100;
                default: return 1;
            }
        }

        /// <summary>
        /// transferredComponent returns the actual selected component microliters in a
        /// proposed transfer. Neither bottle is changed by this planner.
        /// </summary>
        public static MixResult Plan(int total, int targetAmount, int capacity,
            int sourceTotal, int sourceAmount, double percent, int moduleCount,
            Func<int, int> transferredComponent)
        {
            MixResult analytic = MixPlanner.Plan(total, targetAmount, capacity,
                sourceTotal, sourceAmount, percent);
            if ((!analytic.Success && analytic.Failure != MixFailure.PrecisionLimit) || analytic.IsNoOp)
                return analytic;

            double desired = percent / 100.0;
            double current = (double)targetAmount / total;
            double source = (double)sourceAmount / sourceTotal;
            double ideal = (desired * total - targetAmount) / (source - desired);
            int maximum = Math.Min(sourceTotal, capacity - total);
            // Analytic validation permits floating-point noise at an exact resource boundary.
            ideal = Math.Min(ideal, maximum);
            int resolution = ResolutionMicroliters(moduleCount);
            int bestAmount = 0;
            double bestFraction = current;
            double bestError = Math.Abs(current - desired);

            void Consider(long candidate)
            {
                if (candidate <= 0 || candidate > maximum) return;
                int amount = (int)candidate;
                double fraction = ((long)targetAmount + transferredComponent(amount)) /
                    (double)((long)total + amount);
                double error = Math.Abs(fraction - desired);
                if (error < bestError - ComparisonSlack ||
                    (Math.Abs(error - bestError) <= ComparisonSlack && amount < bestAmount))
                {
                    bestAmount = amount;
                    bestFraction = fraction;
                    bestError = error;
                }
            }

            if (resolution == 1)
            {
                long nearest = (long)Math.Round(ideal);
                for (int offset = -32; offset <= 32; offset++)
                    Consider(nearest + offset);
            }
            // Keep every coarser tier's candidates: integer component apportionment
            // can make one of them better than the fine grid's immediate neighbours.
            // All coarser grids are exact multiples of the current resolution.
            for (int level = 0; level <= Math.Max(0, Math.Min(4, moduleCount)); level++)
            {
                int step = ResolutionMicroliters(level);
                Consider((long)Math.Floor(ideal / step) * step);
                Consider((long)Math.Ceiling(ideal / step) * step);
            }

            if (bestAmount == 0)
                return new MixResult(false, 0, MixFailure.PrecisionLimit, current,
                    "The available transfer resolution cannot improve the requested concentration.");

            return new MixResult(true, bestAmount, MixFailure.None, bestFraction,
                "A metered transfer that improves the requested concentration is available.");
        }
    }
}
