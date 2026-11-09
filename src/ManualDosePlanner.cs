// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.

namespace ProbablyStolenMixer
{
    public enum ManualDoseFailure { None, InvalidInput, EmptySource, BelowResolution, InsufficientSource, InsufficientCapacity }

    public readonly struct ManualDoseResult
    {
        public ManualDoseResult(int amount, ManualDoseFailure failure) { Amount = amount; Failure = failure; }
        public int Amount { get; }
        public ManualDoseFailure Failure { get; }
        public bool Success => Failure == ManualDoseFailure.None;
    }

    // Manual dosing deliberately has no component/concentration input or output.
    public static class ManualDosePlanner
    {
        public static ManualDoseResult Plan(int targetVolume, int targetCapacity, int sourceVolume, int requested, int moduleCount)
        {
            if (targetVolume < 0 || targetCapacity < targetVolume || sourceVolume < 0 || requested <= 0)
                return new ManualDoseResult(0, ManualDoseFailure.InvalidInput);
            if (sourceVolume == 0) return new ManualDoseResult(0, ManualDoseFailure.EmptySource);
            int step = MeteredMixPlanner.ResolutionMicroliters(moduleCount);
            int actual = requested / step * step;
            if (actual == 0) return new ManualDoseResult(0, ManualDoseFailure.BelowResolution);
            if (actual > sourceVolume) return new ManualDoseResult(0, ManualDoseFailure.InsufficientSource);
            if (actual > (long)targetCapacity - targetVolume) return new ManualDoseResult(0, ManualDoseFailure.InsufficientCapacity);
            return new ManualDoseResult(actual, ManualDoseFailure.None);
        }
    }
}
