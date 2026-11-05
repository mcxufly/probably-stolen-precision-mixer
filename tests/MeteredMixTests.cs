using System;

namespace ProbablyStolenMixer;

public static class MeteredMixTests
{
    public static void Run()
    {
        Resolutions();
        QuantizedDilutionAndEnrichment();
        ResourceBounds();
        NoOpAndNoImprovement();
        ValidationIsPreserved();
        IntegerApportionment();
        LargeAmounts();
        NestedGridBoundariesAndRandomCases();
    }

    private static void Resolutions()
    {
        int[] expected = { 10000, 5000, 1000, 100, 1 };
        for (int modules = 0; modules <= 4; modules++)
            Equal(expected[modules], MeteredMixPlanner.ResolutionMicroliters(modules), "resolution");
        Equal(10000, MeteredMixPlanner.ResolutionMicroliters(int.MinValue), "negative modules clamp");
        Equal(1, MeteredMixPlanner.ResolutionMicroliters(int.MaxValue), "extra modules clamp");
    }

    private static void QuantizedDilutionAndEnrichment()
    {
        int[] expected = { 20000, 25000, 23000, 23500, 23470 };
        foreach (bool enrich in new[] { false, true })
        {
            int amount = enrich ? 20000 : 80000;
            int sourceAmount = enrich ? 100000 : 0;
            double percent = Desired(100000, amount, 100000, sourceAmount, 23470);
            for (int modules = 0; modules <= 4; modules++)
            {
                var result = PurePlan(100000, amount, 200000, 100000, enrich, percent, modules);
                Transfer(result, expected[modules], 100000, amount, enrich, percent, modules);
                if (modules < 4)
                    Check(Math.Abs(result.PredictedFraction - percent / 100) > MixPlanner.FractionTolerance,
                        "coarse success may exceed analytic concentration tolerance");
            }
        }

        double tiedPercent = 50 * (20000.0 / 120000 + 30000.0 / 130000);
        var tie = PurePlan(100000, 0, 200000, 100000, true, tiedPercent, 0);
        Equal(20000, tie.TransferMicroliters, "equal concentration errors prefer less fluid");
    }

    private static void ResourceBounds()
    {
        int[] expected = { 20000, 20000, 23000, 23400, 23470 };
        double percent = Desired(100000, 0, 23470, 23470, 23470);
        for (int modules = 0; modules <= 4; modules++)
        {
            var sourceBound = PurePlan(100000, 0, 200000, 23470, true, percent, modules);
            var capacityBound = PurePlan(100000, 0, 123470, 100000, true, percent, modules);
            Transfer(sourceBound, expected[modules], 100000, 0, true, percent, modules);
            Transfer(capacityBound, expected[modules], 100000, 0, true, percent, modules);
        }
        Failure(PurePlan(100000, 0, 200000, 23469, true, percent, 0), MixFailure.InsufficientSource);
        Failure(PurePlan(100000, 0, 123469, 100000, true, percent, 0), MixFailure.InsufficientCapacity);
        double smallPercent = Desired(100000, 0, 4000, 4000, 2000);
        Failure(PurePlan(100000, 0, 104000, 4000, true, smallPercent, 0), MixFailure.PrecisionLimit);

        // Exact grid boundaries retain their grid point despite percent conversion noise.
        for (int modules = 0; modules <= 4; modules++)
        {
            int transfer = MeteredMixPlanner.ResolutionMicroliters(modules);
            double desired = Desired(100000, 0, transfer, transfer, transfer);
            var result = PurePlan(100000, 0, 100000 + transfer, transfer, true, desired, modules);
            // One microliter is already inside the existing no-op tolerance at this total.
            Equal(modules == 4 ? 0 : transfer, result.TransferMicroliters, "exact resource boundary");
            Check(result.Success, "exact boundary must succeed");
        }
    }

    private static void NoOpAndNoImprovement()
    {
        var noOp = MeteredMixPlanner.Plan(100000, 50004, 100000, 0, 0, 50, 0,
            _ => throw new Exception("no-op must not apportion source"));
        Check(noOp.IsNoOp, "existing tolerance remains a no-op");
        Equal(0.50004, noOp.PredictedFraction, "no-op fraction");

        double percent = Desired(100000, 0, 100000, 100000, 100);
        Failure(PurePlan(100000, 0, 200000, 100000, true, percent, 0), MixFailure.PrecisionLimit);
        double zeroTiePercent = 50 * 10000.0 / 110000;
        Failure(PurePlan(100000, 0, 200000, 100000, true, zeroTiePercent, 0), MixFailure.PrecisionLimit);

        foreach (bool enrich in new[] { false, true })
        {
            int amount = enrich ? 20000 : 80000;
            double desired = Desired(100000, amount, 100000, enrich ? 100000 : 0, 23470);
            var first = PurePlan(100000, amount, 200000, 100000, enrich, desired, 0);
            Equal(20000, first.TransferMicroliters, "first coarse transfer");
            var second = PurePlan(120000, amount + (enrich ? 20000 : 0), 200000,
                80000, enrich, desired, 0);
            Failure(second, MixFailure.PrecisionLimit);
        }
    }

    private static void ValidationIsPreserved()
    {
        var cases = new (int Total, int Amount, int Capacity, int Source, int Selected, double Percent)[]
        {
            (-1, 0, 100, 100, 100, 50), (100, 101, 200, 100, 100, 50),
            (100, 0, 200, 100, 100, double.NaN), (0, 0, 100, 100, 100, 50),
            (100, 0, 200, 0, 0, 50), (100, 0, 200, 100, 100, 100),
            (100000, 0, 200000, 9999, 9999, 100.0 / 11),
            (100000, 0, 109999, 100000, 100000, 100.0 / 11)
        };
        foreach (var input in cases)
        {
            var expected = MixPlanner.Plan(input.Total, input.Amount, input.Capacity,
                input.Source, input.Selected, input.Percent);
            Check(!expected.Success && expected.Failure != MixFailure.PrecisionLimit, "invalid fixture");
            var actual = MeteredMixPlanner.Plan(input.Total, input.Amount, input.Capacity,
                input.Source, input.Selected, input.Percent, 4,
                _ => throw new Exception("failed validation must not apportion source"));
            Equal(expected.Success, actual.Success, "validation success");
            Equal(expected.Failure, actual.Failure, "validation failure");
            Equal(expected.Reason, actual.Reason, "validation reason unchanged");
            Equal(expected.PredictedFraction, actual.PredictedFraction, "validation fraction unchanged");
            Equal(0, actual.TransferMicroliters, "validation consumes nothing");
        }
    }

    private static void IntegerApportionment()
    {
        // Source [2, 3]: quota floors plus the largest remainder. Ties favor component 0.
        int SplitSelected(int transfer)
        {
            long numerator = 2L * transfer;
            int selected = (int)(numerator / 5);
            int other = 3 * transfer / 5;
            if (selected + other < transfer && numerator % 5 >= 3L * transfer % 5)
                selected++;
            return selected;
        }
        var apportioned = MeteredMixPlanner.Plan(1, 0, 6, 5, 2, 25, 4, SplitSelected);
        Equal(3, apportioned.TransferMicroliters, "full upgrade searches around analytic ideal");
        Check(apportioned.Success, "integer apportionment can rescue analytic precision failure");
        Equal(0.25, apportioned.PredictedFraction, "actual integer apportionment prediction");
        Failure(PurePlan(1, 0, 11, 10, true, 25, 4), MixFailure.PrecisionLimit);

        // Rounded volume is 1, but 2 has a closer concentration; compare actual error.
        double percent = 100 * 1.49999 / 10001.49999;
        var nonlinear = PurePlan(10000, 0, 20000, 10000, true, percent, 4);
        Equal(2, nonlinear.TransferMicroliters, "full upgrade compares concentration errors");
        Check(nonlinear.Success, "neighboring full-upgrade transfer available");
    }

    private static void LargeAmounts()
    {
        for (int modules = 0; modules <= 4; modules++)
        {
            var result = PurePlan(1500000000, 0, int.MaxValue, int.MaxValue, true, 25, modules);
            Transfer(result, 500000000, 1500000000, 0, true, 25, modules);
            result = PurePlan(1000000000, 1000000000, int.MaxValue, int.MaxValue, false, 50, modules);
            Transfer(result, 1000000000, 1000000000, 1000000000, false, 50, modules);
            Failure(PurePlan(int.MaxValue - 1, 0, int.MaxValue, int.MaxValue, true,
                99.999999, modules), MixFailure.InsufficientSource);
        }
        // Ideal and its +32 search neighbors approach Int32.MaxValue without wrapping.
        int available = int.MaxValue - 1;
        double desired = 100.0 * available / int.MaxValue;
        var nearLimit = PurePlan(1, 0, int.MaxValue, available, true, desired, 4);
        Check(nearLimit.Success && nearLimit.TransferMicroliters > 0, "near-int-max search succeeds");
        Check(nearLimit.TransferMicroliters <= available, "near-int-max search respects resources");
    }

    private static void NestedGridBoundariesAndRandomCases()
    {
        foreach (int ideal in new[] { 1, 49, 50, 99, 100, 101, 4999, 5000, 5001,
            9999, 10000, 10001, 23469, 23470, 23471, 99999 })
        {
            CheckNestedGrids(100000, 20000, 150000, 200000, ideal, true);
            CheckNestedGrids(100000, 80000, 150000, 200000, ideal, false);
        }
        var random = new Random(70613);
        for (int test = 0; test < 500; test++)
        {
            int total = random.Next(1000, 1000000);
            int amount = random.Next(total + 1);
            int available = random.Next(1, 1000000);
            int space = random.Next(1, 1000000);
            int ideal = random.Next(1, Math.Min(available, space) + 1);
            CheckNestedGrids(total, amount, available, total + space, ideal, random.Next(2) == 0);
        }
    }

    private static void CheckNestedGrids(int total, int amount, int available, int capacity,
        int ideal, bool enrich)
    {
        double percent = Desired(total, amount, available, enrich ? available : 0, ideal);
        double previousError = Math.Abs((double)amount / total - percent / 100);
        for (int modules = 0; modules <= 4; modules++)
        {
            var result = PurePlan(total, amount, capacity, available, enrich, percent, modules);
            if (!result.Success)
            {
                Equal(MixFailure.PrecisionLimit, result.Failure, "reachable fixture failure");
                continue;
            }
            int transfer = result.TransferMicroliters;
            Check(transfer % MeteredMixPlanner.ResolutionMicroliters(modules) == 0, "whole grid transfer");
            Check(transfer <= available && transfer <= capacity - total, "bounded transfer");
            double predicted = ((long)amount + (enrich ? transfer : 0)) / (double)((long)total + transfer);
            Equal(predicted, result.PredictedFraction, "prediction uses actual component amount");
            double error = Math.Abs(predicted - percent / 100);
            Check(error <= previousError + 1e-15, "nested finer grid cannot worsen concentration");
            if (modules == 4) Check(error <= MixPlanner.FractionTolerance, "full upgrade tolerance");
            previousError = error;
        }
    }

    private static MixResult PurePlan(int total, int amount, int capacity, int available,
        bool enrich, double percent, int modules)
    {
        return MeteredMixPlanner.Plan(total, amount, capacity, available, enrich ? available : 0,
            percent, modules, transfer => enrich ? transfer : 0);
    }

    private static double Desired(int total, int amount, int sourceTotal, int sourceAmount, int ideal)
    {
        return 100 * (amount + (double)sourceAmount / sourceTotal * ideal) / ((long)total + ideal);
    }

    private static void Transfer(MixResult result, int expected, int total, int amount,
        bool enrich, double percent, int modules)
    {
        Check(result.Success, $"expected metered success: {result.Failure}: {result.Reason}");
        Equal(MixFailure.None, result.Failure, "successful failure code");
        Equal(expected, result.TransferMicroliters, "metered transfer");
        Check(expected % MeteredMixPlanner.ResolutionMicroliters(modules) == 0, "grid alignment");
        double predicted = ((long)amount + (enrich ? expected : 0)) / (double)((long)total + expected);
        Equal(predicted, result.PredictedFraction, "actual predicted fraction");
        Check(Math.Abs(predicted - percent / 100) < Math.Abs((double)amount / total - percent / 100),
            "transfer must improve concentration");
    }

    private static void Failure(MixResult result, MixFailure failure)
    {
        Check(!result.Success, "expected failed metered plan");
        Equal(failure, result.Failure, "metered failure code");
        Equal(0, result.TransferMicroliters, "failed metered plan consumes nothing");
        Check(!string.IsNullOrWhiteSpace(result.Reason), "failure supplies plain reason");
    }

    private static void Equal<T>(T expected, T actual, string message)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"{message}: expected {expected}, got {actual}");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
