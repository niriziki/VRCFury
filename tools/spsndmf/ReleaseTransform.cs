using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

// --- Paths (relative to this script file) ---
static string GetScriptDir([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
    Path.GetDirectoryName(path)!;
var scriptDir = GetScriptDir();
var repoRoot = Path.GetFullPath(Path.Combine(scriptDir, "..", ".."));

var generateNewGuids = args.Contains("--generate-guids");
var includeTests = args.Contains("--include-tests");

var sourcePackageDir = Path.Combine(repoRoot, "com.vrcfury.vrcfury");
var guidMapPath = Path.Combine(scriptDir, "guid-map.json");
var outputDir = Path.Combine(repoRoot, "net.nrzk.spsndmf");

// --- Configuration ---
const string NewNamespace = "SpsNdmf";

var asmdefNameMap = new Dictionary<string, string>
{
    ["VRCFury"] = "SPSNDMF",
    ["VRCFury-Runtime-SpsNdmf"] = "SPSNDMF-Runtime-SpsNdmf",
    ["VRCFury-Editor-Common"] = "SPSNDMF-Editor-Common",
    ["VRCFury-Editor-Avatars"] = "SPSNDMF-Editor-Avatars",
    ["VRCFury-Editor-Worlds"] = "SPSNDMF-Editor-Worlds",
    ["VRCFury-Editor-AvatarOptimizer"] = "SPSNDMF-Editor-AvatarOptimizer",
    ["VRCFury-Tests"] = "SPSNDMF-Tests",
    ["VRCFury-Avatar-Tests"] = "SPSNDMF-Avatar-Tests",
    ["com.vrcfury.api"] = "net.nrzk.spsndmf.api",
};

var excludedRootDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "Editor-Worlds",
    "UdonApi",
    "UdonBehaviours",
    "PublicApi",
};
if (!includeTests) excludedRootDirs.Add("Tests");


// --- Validate ---
if (!Directory.Exists(sourcePackageDir))
{
    Console.Error.WriteLine($"Source package directory not found: {sourcePackageDir}");
    Console.Error.WriteLine($"Repo root resolved to: {repoRoot}");
    return 1;
}

Console.WriteLine($"Repo root: {repoRoot}");
Console.WriteLine($"Source:    {sourcePackageDir}");
Console.WriteLine($"Output:    {outputDir}");
Console.WriteLine($"GUID map:  {guidMapPath}");

// --- Step 1: Copy source to output ---
Console.WriteLine("Copying files...");
if (Directory.Exists(outputDir))
    Directory.Delete(outputDir, recursive: true);
CopyPackageRoot(sourcePackageDir, outputDir, excludedRootDirs);

// --- Step 2: Build GUID map ---
Console.WriteLine("Building GUID map...");
var guidMap = LoadOrBuildGuidMap(outputDir, guidMapPath, generateNewGuids);
Console.WriteLine($"  {guidMap.Count} GUIDs mapped");

if (generateNewGuids)
{
    File.WriteAllText(guidMapPath, JsonSerializer.Serialize(guidMap, GuidMapJsonContext.Default.DictionaryStringString));
    Console.WriteLine($"  GUID map saved to {guidMapPath}");
}

// --- Step 3: Process all files ---
Console.WriteLine("Processing files...");
var stats = new TransformStats();

foreach (var filePath in Directory.EnumerateFiles(outputDir, "*", SearchOption.AllDirectories))
{
    var ext = Path.GetExtension(filePath).ToLowerInvariant();
    var fileName = Path.GetFileName(filePath);

    switch (ext)
    {
        case ".cs":
            ProcessCsFile(filePath, guidMap, stats);
            break;
        case ".asmdef":
            ProcessAsmdefFile(filePath, guidMap, stats);
            break;
        case ".meta":
            ProcessMetaFile(filePath, guidMap, stats);
            break;
        case ".asset":
        case ".prefab":
        case ".unity":
        case ".mat":
        case ".controller":
        case ".overridecontroller":
        case ".mask":
            ProcessSerializedFile(filePath, guidMap, stats);
            break;
    }
}

// --- Step 4: Copy CHANGELOG.md template ---
var changelogTemplate = Path.Combine(scriptDir, "CHANGELOG.md");
if (File.Exists(changelogTemplate))
{
    File.Copy(changelogTemplate, Path.Combine(outputDir, "CHANGELOG.md"), overwrite: true);
    File.Copy(changelogTemplate + ".meta", Path.Combine(outputDir, "CHANGELOG.md.meta"), overwrite: true);
    Console.WriteLine("Copied CHANGELOG.md");
}

Console.WriteLine();
Console.WriteLine($"Done! Processed {stats.FilesProcessed} files, {stats.ReplacedFiles} files modified");
Console.WriteLine($"  .cs: {stats.CsFiles} ({stats.CsReplaced} modified)");
Console.WriteLine($"  .asmdef: {stats.AsmdefFiles} ({stats.AsmdefReplaced} modified)");
Console.WriteLine($"  .meta: {stats.MetaFiles} ({stats.MetaReplaced} modified)");
Console.WriteLine($"  serialized: {stats.SerializedFiles} ({stats.SerializedReplaced} modified)");
return 0;

// ============================================================

void CopyDirectory(string source, string dest)
{
    Directory.CreateDirectory(dest);
    foreach (var file in Directory.GetFiles(source))
        File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
    foreach (var dir in Directory.GetDirectories(source))
        CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
}

void CopyPackageRoot(string source, string dest, HashSet<string> excludedTopLevelDirs)
{
    Directory.CreateDirectory(dest);
    foreach (var file in Directory.GetFiles(source))
    {
        var fileName = Path.GetFileName(file);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        if (Path.GetExtension(fileName).Equals(".meta", StringComparison.OrdinalIgnoreCase)
            && excludedTopLevelDirs.Contains(baseName))
            continue;
        File.Copy(file, Path.Combine(dest, fileName));
    }
    foreach (var dir in Directory.GetDirectories(source))
    {
        var dirName = Path.GetFileName(dir);
        if (excludedTopLevelDirs.Contains(dirName)) continue;
        CopyDirectory(dir, Path.Combine(dest, dirName));
    }
}

Dictionary<string, string> LoadOrBuildGuidMap(string packageDir, string mapPath, bool allowGenerate)
{
    Dictionary<string, string> map;
    if (File.Exists(mapPath))
    {
        var json = File.ReadAllText(mapPath);
        map = JsonSerializer.Deserialize(json, GuidMapJsonContext.Default.DictionaryStringString) ?? new();
        Console.WriteLine($"  Loaded {map.Count} existing GUID mappings from {mapPath}");
    }
    else if (!allowGenerate)
    {
        Console.Error.WriteLine($"GUID map not found: {mapPath}");
        Console.Error.WriteLine("Run with --generate-guids to create it.");
        Environment.Exit(1);
        return new(); // unreachable
    }
    else
    {
        map = new();
    }

    var missing = new List<string>();
    foreach (var metaFile in Directory.EnumerateFiles(packageDir, "*.meta", SearchOption.AllDirectories))
    {
        var content = File.ReadAllText(metaFile);
        var match = Regex.Match(content, @"guid:\s*([0-9a-fA-F]{32})");
        if (!match.Success) continue;

        var oldGuid = match.Groups[1].Value.ToLowerInvariant();
        if (!map.ContainsKey(oldGuid))
        {
            if (allowGenerate)
            {
                map[oldGuid] = Guid.NewGuid().ToString("N").ToLowerInvariant();
            }
            else
            {
                missing.Add($"  {oldGuid} in {Path.GetRelativePath(packageDir, metaFile)}");
            }
        }
    }

    if (missing.Count > 0)
    {
        Console.Error.WriteLine($"ERROR: {missing.Count} GUIDs not found in guid-map.json:");
        foreach (var m in missing) Console.Error.WriteLine(m);
        Console.Error.WriteLine("Run with --generate-guids to add them.");
        Environment.Exit(1);
    }

    return map;
}

void ProcessCsFile(string filePath, Dictionary<string, string> guidMap, TransformStats s)
{
    var content = File.ReadAllText(filePath);
    var original = content;

    content = Regex.Replace(content, @"\bnamespace\s+VF\b", $"namespace {NewNamespace}");
    content = Regex.Replace(content, @"\busing\s+VF\.", $"using {NewNamespace}.");
    content = Regex.Replace(content, @"\busing\s+VF;", $"using {NewNamespace};");
    content = Regex.Replace(content, @"(?<![a-zA-Z0-9_])VF\.(?=[A-Z])", $"{NewNamespace}.");

    // Assembly/asmdef names in string literals (InternalsVisibleTo, GetName().Name, etc.)
    foreach (var (oldName, newName) in asmdefNameMap)
        content = content.Replace($"\"{oldName}\"", $"\"{newName}\"");

    // AddComponentMenu: "(VRCFury)" only inside string literals
    content = content.Replace("(VRCFury)\"", "(SPSNDMF)\"");
    content = content.Replace("\"VRCFury/", "\"SPSNDMF/");
    // Editor menu paths under Tools/ or GameObject/ roots
    content = content.Replace("Tools/VRCFury/", "Tools/SPSNDMF/");
    content = content.Replace("GameObject/VRCFury/", "GameObject/SPSNDMF/");

    content = ReplaceGuids(content, guidMap);

    if (content != original)
    {
        File.WriteAllText(filePath, content);
        s.CsReplaced++;
        s.ReplacedFiles++;
    }
    s.CsFiles++;
    s.FilesProcessed++;
}

void ProcessAsmdefFile(string filePath, Dictionary<string, string> guidMap, TransformStats s)
{
    var content = File.ReadAllText(filePath);
    var original = content;

    foreach (var (oldName, newName) in asmdefNameMap)
        content = content.Replace($"\"{oldName}\"", $"\"{newName}\"");

    content = content.Replace("\"rootNamespace\": \"VF\"", $"\"rootNamespace\": \"{NewNamespace}\"");

    content = ReplaceGuids(content, guidMap);

    if (content != original)
    {
        File.WriteAllText(filePath, content);
        s.AsmdefReplaced++;
        s.ReplacedFiles++;
    }
    s.AsmdefFiles++;
    s.FilesProcessed++;
}

void ProcessMetaFile(string filePath, Dictionary<string, string> guidMap, TransformStats s)
{
    var content = File.ReadAllText(filePath);
    var original = content;

    content = Regex.Replace(content, @"(?<=guid:\s*)[0-9a-fA-F]{32}", match =>
        guidMap.TryGetValue(match.Value.ToLowerInvariant(), out var ng) ? ng : match.Value);

    // Upstream folder metas use a legacy minimal format; normalize to Unity's
    // canonical folder meta so shipped packages match what Unity would write.
    if (Directory.Exists(filePath[..^".meta".Length]))
    {
        var guidMatch = Regex.Match(content, @"guid:\s*([0-9a-fA-F]{32})");
        if (guidMatch.Success)
            content = CanonicalFolderMeta(guidMatch.Groups[1].Value);
    }

    if (content != original)
    {
        File.WriteAllText(filePath, content);
        s.MetaReplaced++;
        s.ReplacedFiles++;
    }
    s.MetaFiles++;
    s.FilesProcessed++;
}

void ProcessSerializedFile(string filePath, Dictionary<string, string> guidMap, TransformStats s)
{
    var content = File.ReadAllText(filePath);
    var original = content;

    content = ReplaceGuids(content, guidMap);

    if (content != original)
    {
        File.WriteAllText(filePath, content);
        s.SerializedReplaced++;
        s.ReplacedFiles++;
    }
    s.SerializedFiles++;
    s.FilesProcessed++;
}

string CanonicalFolderMeta(string guid) =>
    "fileFormatVersion: 2\n" +
    $"guid: {guid}\n" +
    "folderAsset: yes\n" +
    "DefaultImporter:\n" +
    "  externalObjects: {}\n" +
    "  userData: \n" +
    "  assetBundleName: \n" +
    "  assetBundleVariant: \n";

string ReplaceGuids(string content, Dictionary<string, string> guidMap)
{
    if (guidMap.Count == 0) return content;
    return Regex.Replace(content, @"[0-9a-fA-F]{32}", match =>
        guidMap.TryGetValue(match.Value.ToLowerInvariant(), out var ng) ? ng : match.Value);
}

class TransformStats
{
    public int FilesProcessed, ReplacedFiles;
    public int CsFiles, CsReplaced;
    public int AsmdefFiles, AsmdefReplaced;
    public int MetaFiles, MetaReplaced;
    public int SerializedFiles, SerializedReplaced;
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
partial class GuidMapJsonContext : JsonSerializerContext { }
