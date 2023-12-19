using System.Diagnostics;
using System.Reflection;
using System.Text;

internal static class Program
{
    private static string _nautilusDirectory;

    private const string VersionPrefixStart = "<VersionPrefix>";
    private const string VersionPrefixEnd = "</VersionPrefix>";
    private const string SuffixNumberStart = "<SuffixNumber>";
    private const string SuffixNumberEnd = "</SuffixNumber>";
    private const string VersionSuffixStart = "<VersionSuffix>";
    private const string VersionSuffixEnd = "</VersionSuffix>";
    private static string _versionPrefix;
    private static int _versionSuffix;
    private static Version _version;

    private static readonly string[] _uploadPageURLs = new string[]
    {
        "https://github.com/SubnauticaModding/Nautilus/releases",
        "https://www.submodica.xyz/mods/sn1/250",
        "https://www.submodica.xyz/mods/sbz/251",
        "https://www.nexusmods.com/subnautica/mods/1262",
        "https://www.nexusmods.com/subnauticabelowzero/mods/373"
    };

    public static void Main(string[] args)
    {
        Console.Clear();
        // essential variables
        _nautilusDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var solutionPath = "";
        while (_nautilusDirectory != null)
        {
            var file = Path.Combine(_nautilusDirectory, "Nautilus.sln");
            if (File.Exists(file))
            {
                solutionPath = file;
                break;
            }

            _nautilusDirectory = Directory.GetParent(_nautilusDirectory)?.FullName;
        }

        if (string.IsNullOrWhiteSpace(solutionPath))
        {
            Console.WriteLine($"Could not find the Nautilus solution in any parent directory.");
            Console.ReadLine();
            return;
        }

        var nautilusProjectPath = Path.Combine(_nautilusDirectory, "Nautilus", "Nautilus.csproj");
        if (!File.Exists(nautilusProjectPath))
        {
            Console.WriteLine($"Could not find the Nautilus project at {nautilusProjectPath}");
            Console.ReadLine();
            return;
        }

        // greeting
        Console.WriteLine("Welcome to the Upload Helper for Nautilus. \nThis program should become obsolete as soon as we set up a proper build deployment system.\n");

        if (!Ask("Do you want to begin? (y/n)"))
        {
            Console.Clear();
            Console.WriteLine("Alright, goodbye!");
            return;
        }

        Console.Clear();

        // get old version
        Console.WriteLine("First of all, we need to determine the version string.");

        Console.WriteLine("You can check https://github.com/SubnauticaModding/Nautilus/releases to find the version that is currently uploaded.");

        if (Ask("Do you want to open this in your browser? (y/n)"))
            Process.Start("explorer", "https://github.com/SubnauticaModding/Nautilus/releases");

        try
        {
            GetCurrentVersionString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.ReadLine();
            return;
        }
        var versionPrefix = _versionPrefix;
        var suffixNumber = _versionSuffix;

        // determine new version
        if (!Ask($"I think the current version string is {versionPrefix}.{suffixNumber}. Do you want to keep it? (y/n)"))
        {
            Console.WriteLine("Alright, we'll change it.");
            Console.WriteLine("Please write the NEW 3 digit version number here (eg: 1.0.0) (do NOT include the pre-release suffix): ");
            versionPrefix = RequestVersion();
            Version v;
            while (!Version.TryParse(versionPrefix, out v) || v.Major < _version.Major || v.Minor < _version.Minor || v.Build < _version.Build)
            {
                Console.Clear();
                Console.WriteLine("That doesn't look like a valid version number or is less than the current version. Please try again.");
                Console.WriteLine("Please write the NEW 3 digit version number here (eg: 1.0.0) (do NOT include the pre-release suffix): ");
                versionPrefix = RequestVersion();
            }

            var isEqual = v.Major == _version.Major && v.Minor == _version.Minor && v.Build == _version.Build;
            var response = isEqual || Ask("Do you want to add a pre-release suffix? (y/n)");
            while (response)
            {
                Console.WriteLine("What should the pre-release suffix be?");
                var prereleaseNum = Console.ReadLine();
                if (string.IsNullOrEmpty(prereleaseNum) || !int.TryParse(prereleaseNum, out suffixNumber))
                {
                    if (!Ask("That doesn't look like an intiger. Do you want to try again? (y/n)"))
                        break;

                    Console.Clear();
                    continue;
                }

                if (isEqual && suffixNumber < _versionSuffix)
                {
                    Console.Clear();
                    Console.WriteLine("That's less than the current pre-release suffix. I'm not sure what you're trying to do, but I'm not going to let you do it.");
                    continue;
                }

                if (suffixNumber < 0)
                {
                    Console.Clear();
                    Console.WriteLine("That's a negative number. I'm not sure what you're trying to do, but I'm not going to let you do it.");
                    continue;
                }

                if (suffixNumber == 0)
                {
                    Console.Clear();
                    Console.WriteLine("That's a zero. I'm not sure what you're trying to do, but I'm not going to let you do it.");
                    if (!Ask("Do you want to try again? (y/n)"))
                        break;
                    continue;
                }

                Console.Clear();
                if (Ask($"\nAdd \"{prereleaseNum}\" as the pre-release suffix? (y/n)"))
                    break;
                Console.Clear();
                response = isEqual || Ask("Do you want to add a pre-release suffix? (y/n)");
            }
        }

        Console.WriteLine($"\nAlright, thanks! We’ll use {versionPrefix}.{suffixNumber} for this release.");
        SetCurrentVersionString(versionPrefix, suffixNumber);

        Console.WriteLine("\nNow, let's work on getting the NuGet packages up and running.");
        Console.WriteLine("I should warn you now that you’ll need to log in to upload your update (for security reasons)." +
            "\nPlease contact an administrator if you need help, otherwise we’ll continue from here.");

        RebuildNautilus(nautilusProjectPath);

        foreach (var url in _uploadPageURLs)
        {
            if (!Ask($"Do you want to open {url} to upload? (y/n)"))
                continue;

            Console.WriteLine("Opening " + url + "...");
            Process.Start("explorer", url);
            Thread.Sleep(500);
            Console.Clear();
        }

        Console.Clear();

        Console.WriteLine("Congratulations, you're done!!!");

        // END
        Console.ReadLine();
    }

    private static void RebuildNautilus(string projectPath)
    {
        // Start a new process
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        process.Start();

        // Define the configurations to build
        var configurations = new string[] { "SN.STABLE", "BZ.STABLE" };

        // Execute the build with nuget restore for each configuration.
        foreach (var configuration in configurations)
            process.StandardInput.WriteLine($"dotnet build \"{projectPath}\" /restore /p:Configuration={configuration}");

        process.StandardInput.WriteLine("exit");

        Console.WriteLine(process.StandardOutput.ReadToEnd());
        process.WaitForExit();
        Console.WriteLine("Now you need to upload the.nupkg files.");

        if (Ask("Do you want to open https://www.nuget.org/packages/manage/upload in your browser? (y/n)"))
            Process.Start("explorer", "https://www.nuget.org/packages/manage/upload");

        if (Ask($"Do you want to open the output paths in explorer? (y/n)"))
            foreach (var configuration in configurations)
                Process.Start("explorer", Path.Combine(_nautilusDirectory, "Nautilus", "bin", configuration));

        Console.WriteLine("Press enter to continue.");
        Console.ReadLine();
    }

    private static string RequestVersion()
    {
        var versionPrefix = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(versionPrefix) && versionPrefix.StartsWith("v"))
        {
            Console.WriteLine("Hm, why does it start with a V? Are you sure you meant to do that? If not, type R and we can retry that.");
            var line = Console.ReadLine();
            if (line != null && line.ToLower() == "r")
            {
                Console.Write("Version number: ");
                versionPrefix = Console.ReadLine();
            }
        }
        else if (string.IsNullOrWhiteSpace(versionPrefix))
        {
            Console.WriteLine("You didn't write anything. I'll assume you want to keep the current version number.");
            versionPrefix = _versionPrefix;
        }

        return versionPrefix;
    }

    private static bool Ask(string prompt)
    {
        Console.WriteLine(prompt);
        var l = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(l) || (l.ToLower() != "y" && l.ToLower() != "n"))
        {
            Console.WriteLine("Please answer with y or n.");
            Console.WriteLine(prompt);
            l = Console.ReadLine();
        }
        return l.ToLower() == "y";
    }

    private static string VersionTargetsPath => Path.Combine(_nautilusDirectory, "Version.targets");

    private static void GetCurrentVersionString()
    {
        var text = File.ReadAllText(VersionTargetsPath);

        var prefixStartIndex = text.IndexOf(VersionPrefixStart) + VersionPrefixStart.Length;
        var prefixLength = text.IndexOf(VersionPrefixEnd) - prefixStartIndex;

        var prefix = text.Substring(prefixStartIndex, prefixLength);
        string suffix = null;

        if (text.Contains(SuffixNumberStart))
        {
            var suffixStartIndex = text.IndexOf(SuffixNumberStart) + SuffixNumberStart.Length;
            var suffixLength = text.IndexOf(SuffixNumberEnd) - suffixStartIndex;
            suffix = text.Substring(suffixStartIndex, suffixLength);
        }

        if (string.IsNullOrWhiteSpace(prefix) || !Version.TryParse(prefix, out _version))
            throw new Exception("The VersionPrefix in Version.targets is not a valid version number. I'm not sure what you're trying to do, but I'm not going to let you do it.");

        _versionPrefix = prefix;
        if (string.IsNullOrWhiteSpace(suffix) || !int.TryParse(suffix, out _versionSuffix))
            throw new Exception("The SuffixNumber in Version.targets is not a number. I'm not sure what you're trying to do, but I'm not going to let you do it.");
    }

    public static void SetCurrentVersionString(string prefix, int suffix)
    {
        var text = File.ReadAllText(VersionTargetsPath);

        var split = text.Split(new string[] { VersionSuffixStart, VersionSuffixEnd, SuffixNumberStart, SuffixNumberEnd, VersionPrefixStart, VersionPrefixEnd }, StringSplitOptions.None);

        var sb = new StringBuilder();
        sb.AppendLine(split[0].TrimEnd());
        sb.AppendLine("        " + VersionPrefixStart + prefix + VersionPrefixEnd);
        sb.AppendLine("        " + SuffixNumberStart + suffix + SuffixNumberEnd);
        if (suffix > 0)
            sb.AppendLine("        " + VersionSuffixStart + "pre.$(SuffixNumber)" + VersionSuffixEnd);

        sb.Append("    " + split[^1].TrimStart());

        var final = sb.ToString();

        File.WriteAllText(VersionTargetsPath, final);
    }
}
