// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
using ProbablyStolenMixer;

public static class ManualAndStockTests
{
    public static void Run()
    {
        int[] steps = { 10000, 5000, 1000, 100, 1 };
        for (int level = 0; level <= 4; level++)
        {
            var plan = ManualDosePlanner.Plan(0, 100000, 100000, 23470, level);
            Check(plan.Success && plan.Amount == 23470 / steps[level] * steps[level], "manual dose floors to grid");
            Check(plan.Amount % steps[level] == 0, "manual dose grid");
        }
        Check(ManualDosePlanner.Plan(0, 1000, 1000, 1000, 4).Amount == 1000, "empty target allowed");
        Check(ManualDosePlanner.Plan(5000, 5000, 10000, 10000, 0).Failure == ManualDoseFailure.InsufficientCapacity, "full target rejects");
        Check(ManualDosePlanner.Plan(0, 100000, 9999, 10000, 0).Failure == ManualDoseFailure.InsufficientSource, "source shortage rejects");
        Check(ManualDosePlanner.Plan(0, 100000, 0, 10000, 0).Failure == ManualDoseFailure.EmptySource, "empty source rejects");
        Check(ManualDosePlanner.Plan(0, 100000, 100000, 9999, 0).Failure == ManualDoseFailure.BelowResolution, "subgrid dose rejects");
        Check(!ManualDosePlanner.Plan(-1, 100, 100, 1, 4).Success, "invalid target rejects");
        Check(!ManualDosePlanner.Plan(0, 100, 100, 0, 4).Success, "zero dose rejects");
        Check(ManualDosePlanner.Plan(0, int.MaxValue, int.MaxValue, int.MaxValue, 4).Amount == int.MaxValue, "int max bounded");
        int available = 0;
        for (int day = 1; day <= 10000; day++)
        {
            bool first = MixerStockPolicy.DailyAvailable("test-run", day);
            Check(first == MixerStockPolicy.DailyAvailable("test-run", day), "deterministic daily outcome");
            if (first) available++;
        }
        Check(available > 2200 && available < 2800, "daily probability approximately25%");
        Check(Enumerable.Range(1, 100).Any(day => MixerStockPolicy.DailyAvailable("run-a", day) != MixerStockPolicy.DailyAvailable("run-b", day)), "runs use distinct schedules");
        Check(MixerStockPolicy.ChancePercent == 25, "25 percent chance");
        Check(MixerStockPolicy.NeedsRoll(false, false, 1, -1), "first day rolls");
        Check(!MixerStockPolicy.NeedsRoll(false, false, 1, 1), "same day never rerolls");
        Check(MixerStockPolicy.NeedsRoll(false, false, 2, 1), "next day rolls");
        foreach (bool availability in new[] { false, true })
        {
            Check(!MixerStockPolicy.ShouldOffer(true, false, availability), "purchased never restocks");
            Check(!MixerStockPolicy.ShouldOffer(false, true, availability), "owned machine not duplicated");
        }
        Check(MixerStockPolicy.ShouldOffer(false, false, true), "unbought daily stock allowed");
        Check(!MixerStockPolicy.ShouldOffer(false, false, false), "failed daily roll no stock");
        Check(MixerStockPolicy.IsPurchaseCandidate("xufly_precision_mixer.machine", "inventor", false), "Jackson purchase candidate");
        Check(!MixerStockPolicy.IsPurchaseCandidate("xufly_precision_mixer.machine", "other", false), "other vendor not recorded");
        Check(!MixerStockPolicy.IsPurchaseCandidate("xufly_precision_mixer.machine", "inventor", true), "already owned not purchase");
        Console.WriteLine("PASS manual dosing and daily one-purchase stock policy.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
}
