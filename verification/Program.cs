using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ProbablyStolenMixer;

class Program
{
    static int Main(string[] args)
    {
        var assembly = typeof(WaterHelper).Assembly;
        foreach (var type in assembly.GetTypes())
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            if (field.Name == "VERSION_NUMBER" || field.Name == "TEXT_VERSION")
                Console.WriteLine("Game version " + type.Name + "." + field.Name + "=" + (field.IsLiteral ? field.GetRawConstantValue() : field.GetValue(null)));
        using (var metadata = DseCecil.AssemblyDefinition.ReadAssembly(assembly.Location))
        foreach (var type in metadata.MainModule.Types)
        foreach (var field in type.Fields)
            if (field.Name == "VERSION_NUMBER") Console.WriteLine("Version metadata " + type.FullName + "=" + field.Constant);
        int checks = 0;
        var random = new Random(12345);
        for (int trial = 0; trial < 10000; trial++)
        {
            var source = Enumerable.Range(0, 12).Select(_ => random.Next(0, 100000)).ToArray();
            int sourceTotal = source.Sum();
            int total = random.Next(10000, 1000000);
            int selected = random.Next(source.Length);
            int targetAmount = random.Next(0, total);
            int added = random.Next(1, sourceTotal + 1);
            double desired = (targetAmount + (double)source[selected] / sourceTotal * added) / (total + added) * 100;
            var plan = MixPlanner.Plan(total, targetAmount, total + sourceTotal, sourceTotal, source[selected], desired);
            if (!plan.Success) throw new Exception("Reachable plan failed: " + plan.Reason);
            if (plan.IsNoOp) continue;
            int[] moved = WaterHelper.SplitProportionally(source, plan.TransferMicroliters);
            if (moved.Sum() != plan.TransferMicroliters) throw new Exception("Volume not conserved");
            for (int i = 0; i < source.Length; i++)
                if (moved[i] < 0 || moved[i] > source[i]) throw new Exception("Component not conserved");
            double fraction = ((long)targetAmount + moved[selected]) / (double)(total + (long)plan.TransferMicroliters);
            if (Math.Abs(fraction - desired / 100) > MixPlanner.FractionTolerance)
                throw new Exception("Actual game rounding exceeded tolerance");
            checks++;
        }
        Console.WriteLine("Actual WaterHelper.SplitProportionally: " + checks + " transfer cases passed.");
        var tiny = WaterHelper.SplitProportionally(new[] { 2, 3 }, 3);
        if (tiny[0] != 1 || tiny[1] != 2 || tiny[0] / 4.0 != 0.25) throw new Exception("Tiny-volume rounding regression");
        Console.WriteLine("Tiny-volume rounding regression passed: 1uL + 3uL yields exactly 25%.");
        string managed = Path.GetDirectoryName(assembly.Location)!;
        string game = "/data/SteamLibrary/steamapps/common/Probably Stolen/Probably Stolen_Data/Managed";
        var allowed = System.IO.Directory.GetFiles(game, "*.dll").Select(Path.GetFileNameWithoutExtension).ToArray();
        string folder = Path.GetFullPath("dist/PrecisionMixer");
        var manifest = ModLoader.ParseManifestForTools(folder, out string problem);
        if (manifest == null || problem != null) throw new Exception("Manifest invalid: " + problem);
        Console.WriteLine("Game manifest parser passed: " + manifest.ID + " / " + manifest.ModVersion);
        var scan = ModScanner.Scan(new[] { Path.Combine(folder, "ProbablyStolenMixer.dll") }, allowed, folder, false);
        Console.WriteLine("Scanner: " + scan.MethodsScanned + " methods; capabilities=" + string.Join(",", scan.Capabilities));
        Console.WriteLine(scan.FormatFindings(100));
        if (scan.Error != null) Console.WriteLine(scan.Error);
        if (scan.Refused || scan.Capabilities.Count != 0) return 1;
        Console.WriteLine("Game scanner passed with no capabilities or hard flags.");
        return 0;
    }
}
