// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.

using System;

namespace ProbablyStolenMixer
{
    public enum MixFailure
    {
        None,
        InvalidInput,
        EmptyTarget,
        EmptySource,
        Unreachable,
        InsufficientSource,
        InsufficientCapacity,
        PrecisionLimit
    }

    public readonly struct MixResult
    {
        internal MixResult(bool success, int transferMicroliters, MixFailure failure,
            double predictedFraction, string reason)
        {
            Success = success;
            TransferMicroliters = transferMicroliters;
            Failure = failure;
            PredictedFraction = predictedFraction;
            Reason = reason;
        }

        public bool Success { get; }
        public bool IsNoOp { get { return Success && TransferMicroliters == 0; } }
        public int TransferMicroliters { get; }
        public MixFailure Failure { get; }
        public double PredictedFraction { get; }
        public string Reason { get; }
    }

    /// <summary>Plans addition of homogeneous source fluid without mutating either bottle.</summary>
    public static class MixPlanner
    {
        public const double FractionTolerance = 0.00005;

        /// <summary>
        /// All amounts are integer microliters. targetAmount and sourceAmount refer to
        /// the same component index; percent is the desired concentration from 0 to 100.
        /// The prediction assumes a continuous, homogeneous source. The caller must
        /// validate the game's integer component apportionment before transferring.
        /// </summary>
        public static MixResult Plan(int total, int targetAmount, int capacity,
            int sourceTotal, int sourceAmount, double percent)
        {
            if (total < 0 || targetAmount < 0 || capacity < 0 || sourceTotal < 0 ||
                sourceAmount < 0 || targetAmount > total || sourceAmount > sourceTotal ||
                total > capacity || double.IsNaN(percent) || double.IsInfinity(percent) ||
                percent < 0 || percent > 100)
            {
                return Fail(MixFailure.InvalidInput, double.NaN,
                    "Amounts must describe valid bottles and the desired percent must be finite and between 0 and 100.");
            }

            if (total == 0)
                return Fail(MixFailure.EmptyTarget, double.NaN, "The target bottle is empty.");

            double current = (double)targetAmount / total;
            double desired = percent / 100.0;
            if (Math.Abs(current - desired) <= FractionTolerance)
                return new MixResult(true, 0, MixFailure.None, current,
                    "The target is already within the requested concentration tolerance.");

            if (sourceTotal == 0)
                return Fail(MixFailure.EmptySource, current, "The source bottle is empty.");

            double source = (double)sourceAmount / sourceTotal;
            // A finite addition can only reach concentrations strictly between the
            // current concentration and source concentration. The source endpoint
            // itself would require replacing/discarding the existing target fluid.
            if (desired <= Math.Min(current, source) || desired >= Math.Max(current, source))
                return Fail(MixFailure.Unreachable, current,
                    "Adding this source cannot reach the desired concentration with a finite transfer.");

            double required = (desired * total - targetAmount) / (source - desired);
            // Compare before converting to an integer: near an endpoint the required
            // transfer may greatly exceed Int32.MaxValue. No partial plan is returned.
            // Permit only floating-point noise at an exact boundary, not a
            // concentration-tolerance-sized shortage. Inverting percent can turn
            // an exact transfer of 2 into 2.0000000000000004, for example.
            const double arithmeticTolerance = 1e-15;
            if (required > sourceTotal &&
                Math.Abs(Predict(total, targetAmount, source, sourceTotal) - desired) > arithmeticTolerance)
                return Fail(MixFailure.InsufficientSource, current,
                    "The source does not contain enough fluid for the required transfer.");

            long space = (long)capacity - total;
            if (required > space &&
                Math.Abs(Predict(total, targetAmount, source, (int)space) - desired) > arithmeticTolerance)
                return Fail(MixFailure.InsufficientCapacity, current,
                    "The target bottle does not have enough space for the required transfer.");

            required = Math.Min(required, Math.Min(sourceTotal, space));
            int lower = (int)Math.Floor(required);
            int upper = (int)Math.Ceiling(required);
            double lowerFraction = Predict(total, targetAmount, source, lower);
            double upperFraction = Predict(total, targetAmount, source, upper);
            double lowerError = Math.Abs(lowerFraction - desired);
            double upperError = Math.Abs(upperFraction - desired);
            // Compare concentration errors rather than rounding x alone: the
            // concentration changes nonlinearly with the added volume. Prefer the
            // smaller transfer when the two predictions are equally accurate.
            int transfer = lowerError <= upperError ? lower : upper;
            double predicted = lowerError <= upperError ? lowerFraction : upperFraction;
            if (Math.Abs(predicted - desired) > FractionTolerance)
                return Fail(MixFailure.PrecisionLimit, current,
                    "No neighboring integer-microliter transfer reaches the requested concentration tolerance.");

            return new MixResult(true, transfer, MixFailure.None, predicted,
                "A transfer within the requested concentration tolerance is available.");
        }

        private static double Predict(int total, int targetAmount, double source, int transfer)
        {
            return (targetAmount + source * transfer) / ((long)total + transfer);
        }

        private static MixResult Fail(MixFailure failure, double current, string reason)
        {
            return new MixResult(false, 0, failure, current, reason);
        }
    }
}
