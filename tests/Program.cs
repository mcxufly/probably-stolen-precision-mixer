using ProbablyStolenMixer;

var tests = new (string Name, Action Run)[]
{
    ("dilution preserves component amount", () =>
    {
        var result = MixPlanner.Plan(1000, 800, 3000, 2000, 0, 40);
        Transfer(result, 1000, 0.4);
        Equal(800.0, result.PredictedFraction * 2000, "component conserved");
    }),
    ("pure source enrichment", () => Transfer(MixPlanner.Plan(1000, 200, 3000, 2000, 2000, 60), 1000, 0.6)),
    ("mixed source enrichment", () => Transfer(MixPlanner.Plan(1000, 200, 5000, 3000, 2400, 50), 1000, 0.5)),
    ("mixed source dilution", () => Transfer(MixPlanner.Plan(1000, 800, 5000, 3000, 600, 50), 1000, 0.5)),
    ("exact desired fraction permits no-op with empty source and full target", () =>
    {
        var result = MixPlanner.Plan(1000, 500, 1000, 0, 0, 50);
        Transfer(result, 0, 0.5);
        Check(result.IsNoOp, "expected no-op");
    }),
    ("within tolerance permits no-op", () => Transfer(MixPlanner.Plan(100000, 50004, 100000, 0, 0, 50), 0, 0.50004)),
    ("no-op endpoints", () =>
    {
        Transfer(MixPlanner.Plan(1000, 0, 1000, 0, 0, 0), 0, 0);
        Transfer(MixPlanner.Plan(1000, 1000, 1000, 0, 0, 100), 0, 1);
    }),
    ("empty target", () => Failure(MixPlanner.Plan(0, 0, 1000, 1000, 500, 50), MixFailure.EmptyTarget)),
    ("empty source", () => Failure(MixPlanner.Plan(1000, 200, 3000, 0, 0, 50), MixFailure.EmptySource)),
    ("no target space", () => Failure(MixPlanner.Plan(1000, 200, 1000, 2000, 2000, 60), MixFailure.InsufficientCapacity)),
    ("insufficient capacity is not a partial success", () => Failure(MixPlanner.Plan(1000, 200, 1999, 2000, 2000, 60), MixFailure.InsufficientCapacity)),
    ("insufficient source is not a partial success", () => Failure(MixPlanner.Plan(1000, 200, 3000, 999, 999, 60), MixFailure.InsufficientSource)),
    ("fractional shortage still fails despite nearby integer", () =>
    {
        Failure(MixPlanner.Plan(10000, 2000, 50000, 10000, 10000, 60.0001), MixFailure.InsufficientSource);
        Failure(MixPlanner.Plan(10000, 2000, 20000, 20000, 20000, 60.0001), MixFailure.InsufficientCapacity);
    }),
    ("source endpoint requires infinite transfer", () =>
    {
        Failure(MixPlanner.Plan(1000, 200, 100000, 100000, 80000, 80), MixFailure.Unreachable);
        Failure(MixPlanner.Plan(1000, 800, 100000, 100000, 20000, 20), MixFailure.Unreachable);
        Failure(MixPlanner.Plan(1000, 200, int.MaxValue, int.MaxValue, int.MaxValue, 100), MixFailure.Unreachable);
        Failure(MixPlanner.Plan(1000, 800, int.MaxValue, int.MaxValue, 0, 0), MixFailure.Unreachable);
    }),
    ("equal source cannot change concentration", () => Failure(MixPlanner.Plan(1000, 200, 10000, 10000, 2000, 50), MixFailure.Unreachable)),
    ("wrong direction and beyond endpoint", () =>
    {
        Failure(MixPlanner.Plan(1000, 200, 10000, 10000, 8000, 10), MixFailure.Unreachable);
        Failure(MixPlanner.Plan(1000, 800, 10000, 10000, 2000, 90), MixFailure.Unreachable);
        Failure(MixPlanner.Plan(1000, 200, 10000, 10000, 8000, 90), MixFailure.Unreachable);
    }),
    ("small bottle cannot meet microliter precision", () => Failure(MixPlanner.Plan(1, 0, 10, 10, 10, 25), MixFailure.PrecisionLimit)),
    ("round upward within tolerance", () =>
    {
        double desired = 100 * 1000.6 / 11000.6;
        var result = MixPlanner.Plan(10000, 0, 20000, 10000, 10000, desired);
        Transfer(result, 1001, 1001.0 / 11001);
        Check(Math.Abs(result.PredictedFraction - desired / 100) <= MixPlanner.FractionTolerance, "tolerance");
    }),
    ("round downward within tolerance", () =>
    {
        double desired = 100 * 1000.4 / 11000.4;
        Transfer(MixPlanner.Plan(10000, 0, 20000, 10000, 10000, desired), 1000, 1000.0 / 11000);
    }),
    ("nearest concentration may differ from nearest volume", () =>
    {
        // x=1.49999 rounds down by volume, but 2 uL gives a slightly
        // closer concentration because concentration is nonlinear in x.
        double desired = 100 * 1.49999 / 10001.49999;
        Transfer(MixPlanner.Plan(10000, 0, 20000, 10000, 10000, desired), 2, 2.0 / 10002);
    }),
    ("malformed input is rejected before no-op", () =>
    {
        foreach (var args in new[]
        {
            (-1, 0, 100, 100, 100), (100, -1, 100, 100, 100),
            (100, 101, 200, 100, 100), (100, 50, -1, 100, 100),
            (100, 50, 99, 100, 100), (100, 50, 200, -1, 0),
            (100, 50, 200, 100, -1), (100, 50, 200, 100, 101),
            (100, 50, 200, 0, 1), (0, 1, 200, 100, 100)
        })
            Failure(MixPlanner.Plan(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5, 50), MixFailure.InvalidInput);
        foreach (double percent in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, -0.01, 100.01 })
            Failure(MixPlanner.Plan(100, 50, 200, 100, 100, percent), MixFailure.InvalidInput);
    }),
    ("large integer amounts do not overflow", () =>
    {
        Transfer(MixPlanner.Plan(1500000000, 0, int.MaxValue, int.MaxValue, int.MaxValue, 25), 500000000, 0.25);
        Transfer(MixPlanner.Plan(1000000000, 1000000000, int.MaxValue, int.MaxValue, 0, 50), 1000000000, 0.5);
        Failure(MixPlanner.Plan(int.MaxValue - 1, 0, int.MaxValue, int.MaxValue, int.MaxValue, 99.999999), MixFailure.InsufficientSource);
    }),
    ("representative exact boundary transfers", () =>
    {
        for (int total = 1; total <= 100; total++)
        {
            for (int available = 1; available <= 100; available++)
            {
                double percent = 100.0 * available / (total + available);
                var result = MixPlanner.Plan(total, 0, total + available, available, available, percent);
                Check(result.Success, $"boundary total={total}, source={available}: {result.Failure}");
                Equal(available, result.TransferMicroliters, "exact boundary transfer");
            }
        }
    }),
    ("predictions and resources agree across volume combinations", () =>
    {
        foreach (int total in new[] { 1000, 10000, 1000000 })
        foreach (int initialPercent in new[] { 0, 20, 50, 80, 100 })
        foreach (int sourcePercent in new[] { 0, 20, 50, 80, 100 })
        foreach (int transfer in new[] { 11, 137, 999 })
        {
            int amount = total * initialPercent / 100;
            int sourceTotal = 100000;
            int sourceAmount = sourceTotal * sourcePercent / 100;
            double desired = 100.0 * (amount + transfer * sourcePercent / 100.0) / (total + transfer);
            var result = MixPlanner.Plan(total, amount, total + 10000, sourceTotal, sourceAmount, desired);
            Check(result.Success, $"known reachable mixture failed: {result.Failure}");
            Check(result.TransferMicroliters >= 0 && result.TransferMicroliters <= 10000, "capacity respected");
            Check(result.TransferMicroliters <= sourceTotal, "source respected");
            double predicted = (amount + result.TransferMicroliters * (sourceAmount / (double)sourceTotal)) / ((long)total + result.TransferMicroliters);
            Near(predicted, result.PredictedFraction, "prediction");
            Check(Math.Abs(predicted - desired / 100) <= MixPlanner.FractionTolerance, "success must meet tolerance");
            if (!result.IsNoOp)
            {
                double chosenError = Math.Abs(predicted - desired / 100);
                foreach (int neighbor in new[] { result.TransferMicroliters - 1, result.TransferMicroliters + 1 })
                {
                    double alternative = (amount + neighbor * (sourceAmount / (double)sourceTotal)) / ((long)total + neighbor);
                    Check(chosenError <= Math.Abs(alternative - desired / 100) + 1e-15, "chosen transfer is nearest concentration");
                }
            }
        }
    })
};

int failures = 0;
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception error)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {test.Name}: {error.Message}");
    }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} test groups passed.");
return failures == 0 ? 0 : 1;

static void Transfer(MixResult result, int expected, double fraction)
{
    Check(result.Success, $"expected success, got {result.Failure}: {result.Reason}");
    Equal(MixFailure.None, result.Failure, "success failure code");
    Equal(expected, result.TransferMicroliters, "transfer");
    Near(fraction, result.PredictedFraction, "fraction");
    Check(!string.IsNullOrWhiteSpace(result.Reason), "plain English reason");
}

static void Failure(MixResult result, MixFailure expected)
{
    Check(!result.Success, "expected failure");
    Equal(expected, result.Failure, "failure code");
    Equal(0, result.TransferMicroliters, "failure cannot transfer fluid");
    Check(!result.IsNoOp, "failure is not successful no-op");
    Check(!string.IsNullOrWhiteSpace(result.Reason), "plain English reason");
}

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

static void Equal<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"{message}: expected {expected}, got {actual}");
}

static void Near(double expected, double actual, string message)
{
    Check(Math.Abs(expected - actual) < 1e-12, $"{message}: expected {expected:R}, got {actual:R}");
}
