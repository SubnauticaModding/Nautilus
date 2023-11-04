using System.Diagnostics;
using System.Reflection;
using System.Text;

internal static class Program
{
    private static string _nautilusDirectory;

    private const string VersionPrefixStart = "<VersionPrefix>";
    private const string VersionPrefixEnd = "</VersionPrefix>";
    private const string VersionSuffixStart = "<VersionSuffix>";
    private const string VersionSuffixEnd = "</VersionSuffix>";

    private static string[] _uploadPageURLs = new string[]
    {
        "https://github.com/SubnauticaModding/Nautilus/releases",
        "https://www.submodica.xyz/mods/sn1/250",
        "https://www.submodica.xyz/mods/sbz/251",
        "https://www.nexusmods.com/subnautica/mods/1262",
        "https://www.nexusmods.com/subnauticabelowzero/mods/373"
    };

    public static void Main(string[] args)
    {
        // essential variables
        _nautilusDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..");

        // greeting
        Console.WriteLine("Welcome to the Upload Helper for Nautilus. This program should become obsolete as soon as we set up a proper build deployment system.\n");

        Console.WriteLine("Press ENTER to begin...");
        Console.ReadLine();

        // get old version
        Console.WriteLine("First of all, we need to determine the version string.");
        var oldVersion = GetCurrentVersionString();
        Console.WriteLine($"I think the current version string is {oldVersion} (suffixes such as the pre-release number might not be shown here), but it's a good idea to double check.\n");
        Console.WriteLine("When you press enter, I will open https://www.submodica.xyz/mods/sn1/246 and https://github.com/SubnauticaModding/Nautilus/releases in your browser so you can find the version string that is currently uploaded to the internet.");
        Console.WriteLine("If you don't want this, just type something before you hit enter.");
        if (string.IsNullOrEmpty(Console.ReadLine()))
        {
            Process.Start("explorer", "https://www.submodica.xyz/mods/sn1/250");
            Thread.Sleep(1000);
            Process.Start("explorer", "https://github.com/SubnauticaModding/Nautilus/releases");
        }

        // determine new version

        Console.WriteLine("Please write the NEW version number here (do NOT include the pre-release suffix): ");
        var versionPrefix = Console.ReadLine();
        if (versionPrefix != null && versionPrefix.StartsWith("v"))
        {
            Console.WriteLine("Hm, why does it start with a V? Are you sure you meant to do that? If not, type R and we can retry that.");
            var line = Console.ReadLine();
            if (line != null && line.ToLower() == "r")
            {
                Console.Write("Version number: ");
                versionPrefix = Console.ReadLine();
            }
        }

        Console.WriteLine("\nAnd now, if applicable, send the pre-release number (or leave empty): ");
        string prereleaseNum = Console.ReadLine();
        string versionString = versionPrefix;

        if (!string.IsNullOrEmpty(prereleaseNum))
        {
            versionString += "-pre." + prereleaseNum;
            Console.WriteLine($"\nOh, you want to add \"pre.{prereleaseNum}\" to the version suffix? Sure, just remember we have to remove that when making the builds that we distribute.");
            Console.WriteLine("If you are wondering why, that is because the BepInEx plugin.");
        }

        Console.WriteLine($"\nAlright, thanks! We’ll use {versionString} for this release.");

        SetCurrentVersionString(versionPrefix, "pre." + prereleaseNum);

        Console.WriteLine("\nThe Version.targets file was automatically updated. Remember that we have to fix that later.");
        Console.WriteLine("\nNow, let's work on getting the NuGet package up and running.");

        Console.WriteLine("I should warn you now that you’ll need to log in to upload your update (for security reasons)." +
            "\nPlease contact an administrator if you need help, otherwise we’ll continue from here.");

        WalkThroughNuGetSteps("SN.STABLE");

        WalkThroughNuGetSteps("BZ.STABLE");

        Console.WriteLine("\nRemember, these two versions you built are ONLY used for NUGET DEPENDENCIES. The version you have now should NOT be used in-game.");

        Console.WriteLine("Alright, great, did everything work out? I’m having connection issues and I can’t see your responses, so I’ll assume that’s a yes.");

        SetCurrentVersionString(versionPrefix, null);

        Console.WriteLine("\nNow let’s work on getting this update pushed out to our users. First, we need to remove the prerelease tags from the assembly." +
            "\nI’ve done that for you already. Now build for BOTH versions of the game.");

        Console.WriteLine("Press enter after you have ONCE AGAIN built the project for both SN.STABLE and BZ.STABLE.");

        Console.ReadLine();

        Console.WriteLine("Press enter and I will open all the relevant pages where these mods should be uploaded in your browser.");

        Console.ReadLine();

        foreach (var url in _uploadPageURLs)
        {
            Console.WriteLine("Opening " + url + "...");
            Process.Start("explorer", url);
            Thread.Sleep(500);
        }

        Console.WriteLine("Congratulations, you're done!!!");

        // END
        Console.ReadLine();
    }

    private static bool Ask(string prompt)
    {
        Console.WriteLine(prompt);
        var l = Console.ReadLine();
        return !string.IsNullOrEmpty(l) && l.ToLower() == "y";
    }

    private static string VersionTargetsPath => Path.Combine(_nautilusDirectory, "Version.targets");

    private static void RebuildAuthorsTable()
    {
        var proc = new Process();
        proc.StartInfo.FileName = Path.Combine(_nautilusDirectory, "AuthorsTableGenerator", "AuthorTableGenerator.exe");
        proc.Start();
        proc.WaitForExit();
        proc.Close();
    }

    private static void WalkThroughNuGetSteps(string branch)
    {
        Console.WriteLine($"\nIn your IDE, switch to the {branch} build configuration, build the project, and then press ENTER in this window when you have finished.");

        Console.ReadLine();

        Console.WriteLine("All built and ready? When you press ENTER I will open the folder containing the built files.");

        Console.ReadLine();

        Process.Start("explorer", Path.Combine(_nautilusDirectory, "Nautilus", "bin", branch));

        Console.WriteLine("Got it? Now you need to upload the correct .nupkg file at https://www.nuget.org/packages/manage/upload. Press ENTER to open that in your browser.");

        Console.ReadLine();

        Process.Start("explorer", "https://www.nuget.org/packages/manage/upload");
    }

    private static string GetCurrentVersionString()
    {
        var text = File.ReadAllText(VersionTargetsPath);

        var prefixStartIndex = text.IndexOf(VersionPrefixStart) + VersionPrefixStart.Length;
        var prefixLength = text.IndexOf(VersionPrefixEnd) - prefixStartIndex;

        string prefix = text.Substring(prefixStartIndex, prefixLength);
        string suffix = null;

        if (text.Contains(VersionSuffixStart))
        {
            var suffixStartIndex = text.IndexOf(VersionSuffixStart) + VersionSuffixStart.Length;
            var suffixLength = text.IndexOf(VersionSuffixEnd) - suffixStartIndex;
            suffix = text.Substring(suffixStartIndex, suffixLength);
        }

        if (string.IsNullOrEmpty(suffix)) return prefix;

        else return prefix + "-" + suffix;
    }

    public static void SetCurrentVersionString(string prefix, string suffix = null)
    {
        var text = File.ReadAllText(VersionTargetsPath);

        var split = text.Split(new string[] { VersionSuffixStart, VersionSuffixEnd, VersionPrefixStart, VersionPrefixEnd }, StringSplitOptions.None);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(split[0].TrimEnd());
        sb.AppendLine("        " + VersionPrefixStart + prefix + VersionPrefixEnd);
        if (!string.IsNullOrEmpty(suffix))
        {
            sb.AppendLine("        " + VersionSuffixStart + suffix + VersionSuffixEnd);
        }
        sb.Append("    " + split[split.Length - 1].TrimStart());

        var final = sb.ToString();

        File.WriteAllText(VersionTargetsPath, final);
    }
}
