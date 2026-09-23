using System.Text.RegularExpressions;

// --- Paths (relative to this script file) ---
static string GetScriptDir([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
    Path.GetDirectoryName(path)!;
var scriptDir = GetScriptDir();
var repoRoot = Path.GetFullPath(Path.Combine(scriptDir, "..", ".."));

var sourcePackageDir = Path.Combine(repoRoot, "com.vrcfury.vrcfury");
var sourceRuntimeDir = Path.Combine(sourcePackageDir, "Runtime");
var sourceRuntimeMeta = Path.Combine(sourcePackageDir, "Runtime.meta");
var sourceApiDir = Path.Combine(sourcePackageDir, "PublicApi");
var sourceApiMeta = Path.Combine(sourcePackageDir, "PublicApi.meta");
var excludeListPath = Path.Combine(scriptDir, "exclude-files.txt");
var editorAvatarsIncludePath = Path.Combine(scriptDir, "editor-avatars-include.txt");
var stripVfInitPath = Path.Combine(scriptDir, "strip-vfinit.txt");
var apiAsmdefTemplate = Path.Combine(scriptDir, "com.vrcfury.api.asmdef");
var avatarsAsmdefTemplate = Path.Combine(scriptDir, "VRCFury-Editor-Avatars.asmdef");
var stubEditorDir = Path.Combine(scriptDir, "Editor");
var outputDir = Path.Combine(repoRoot, "net.nrzk.vfstub");
var outputRuntimeDir = Path.Combine(outputDir, "Runtime");
var outputApiDir = Path.Combine(outputDir, "PublicApi");

// --- Configuration ---
// asmdef names are kept as-is ("VRCFury", "VRCFury-Editor-*") so [SerializeReference] type
// references in existing assets (which embed the assembly name) resolve without MovedFrom
// attributes. The stub is drop-in compatible and mutually exclusive with the real VRCFury package.
const string OldMenuLabel = "VRCFury";
const string NewMenuLabel = "VRCFuryStub";

// Editor resources the stub must not share with SPSNDMF in the same project. Exact tokens;
// the transform fails if one is not found so an upstream rename cannot silently re-share them.
var editorRewrites = new (string Old, string New)[]
{
    ("\"Tools/VRCFury/", "\"Tools/VRCFury Stub/"),
    ("new Harmony(\"com.vrcfury.harmony\")", "new Harmony(\"com.vrcfury.stub.harmony\")"),
    ("\"ProjectSettings/SpsNdmfPrefabInstanceMode.asset\"", "\"ProjectSettings/VfStubPrefabInstanceMode.asset\""),
    // VRCFPackageUtils.Version: upstream package.json GUID -> the stub's package.json GUID
    ("GetVersionFromGuid(\"da4518ec79a04334b86a18805f1b8d24\")", "GetVersionFromGuid(\"60179d52e3b0ec4671ce15992ed99c34\")"),
    ("new Label(\"SPSNDMF\")", "new Label(\"VRCFuryStub\")"),
};

// --- Validate ---
if (!Directory.Exists(sourceRuntimeDir))
{
    Console.Error.WriteLine($"Source Runtime directory not found: {sourceRuntimeDir}");
    return 1;
}

Console.WriteLine($"Repo root: {repoRoot}");
Console.WriteLine($"Source:    {sourcePackageDir}");
Console.WriteLine($"Output:    {outputDir}");

// --- Step 1: Clean and copy Runtime ---
Console.WriteLine("Copying Runtime...");
if (Directory.Exists(outputDir))
    Directory.Delete(outputDir, recursive: true);
Directory.CreateDirectory(outputDir);
CopyDirectory(sourceRuntimeDir, outputRuntimeDir);
File.Copy(sourceRuntimeMeta, Path.Combine(outputDir, "Runtime.meta"));

// --- Step 1b: Copy PublicApi and swap in the stub asmdef ---
// The asmdef name and GUID stay identical to upstream so tools that reference
// com.vrcfury.api by name or GUID resolve it. The template gates the assembly on
// SPSNDMF + migrator so the API only exists where the written data takes effect.
Console.WriteLine("Copying PublicApi...");
CopyDirectory(sourceApiDir, outputApiDir);
File.Copy(sourceApiMeta, Path.Combine(outputDir, "PublicApi.meta"));
File.Copy(apiAsmdefTemplate, Path.Combine(outputApiDir, "com.vrcfury.api.asmdef"), overwrite: true);

// --- Step 1c: Copy the inspector: Editor-Common wholesale, Editor-Avatars by include list ---
// Editor-Common keeps its upstream asmdef; Editor-Avatars gets a template without the
// SPSNDMF-only references. See .agent-docs/vfstub-editor-bundle-policy.md for what is kept and why.
Console.WriteLine("Copying Editor-Common...");
CopyDirectory(Path.Combine(sourcePackageDir, "Editor-Common"), Path.Combine(outputDir, "Editor-Common"));
File.Copy(Path.Combine(sourcePackageDir, "Editor-Common.meta"), Path.Combine(outputDir, "Editor-Common.meta"));
CopyDirectory(Path.Combine(sourcePackageDir, "VrcfResources"), Path.Combine(outputDir, "VrcfResources"));
File.Copy(Path.Combine(sourcePackageDir, "VrcfResources.meta"), Path.Combine(outputDir, "VrcfResources.meta"));
foreach (var file in Directory.GetFiles(stubEditorDir))
    File.Copy(file, Path.Combine(outputDir, "Editor-Common", "Inspector", Path.GetFileName(file)));

Console.WriteLine("Copying Editor-Avatars subset...");
var avatarsInclude = LoadList(editorAvatarsIncludePath);
foreach (var rel in avatarsInclude)
{
    var src = Path.Combine(sourcePackageDir, rel);
    if (!File.Exists(src) || !File.Exists(src + ".meta"))
    {
        Console.Error.WriteLine($"editor-avatars-include.txt entry not found in source (renamed or removed upstream?): {rel}");
        return 1;
    }
    CopyWithParentMetas(sourcePackageDir, outputDir, rel);
}
File.Copy(avatarsAsmdefTemplate, Path.Combine(outputDir, "Editor-Avatars", "VRCFury-Editor-Avatars.asmdef"));
File.Copy(Path.Combine(sourcePackageDir, "Editor-Avatars", "VRCFury-Editor-Avatars.asmdef.meta"),
    Path.Combine(outputDir, "Editor-Avatars", "VRCFury-Editor-Avatars.asmdef.meta"));
Console.WriteLine($"  {avatarsInclude.Count} files included");

// --- Step 1d: Drop excluded files ---
var excludedFiles = LoadList(excludeListPath);
foreach (var rel in excludedFiles)
{
    var target = Path.Combine(outputDir, rel);
    if (!File.Exists(target) || !File.Exists(target + ".meta"))
    {
        Console.Error.WriteLine($"exclude-files.txt entry not found in source (renamed or removed upstream?): {rel}");
        return 1;
    }
    File.Delete(target);
    File.Delete(target + ".meta");
}
Console.WriteLine($"  {excludedFiles.Count} files excluded");

// Upstream folder metas use a legacy minimal format; normalize to Unity's
// canonical folder meta so shipped packages match what Unity would write.
foreach (var metaFile in Directory.EnumerateFiles(outputDir, "*.meta", SearchOption.AllDirectories))
{
    if (!Directory.Exists(metaFile[..^".meta".Length])) continue;
    var guidMatch = Regex.Match(File.ReadAllText(metaFile), @"guid:\s*([0-9a-fA-F]{32})");
    if (guidMatch.Success)
        File.WriteAllText(metaFile, CanonicalFolderMeta(guidMatch.Groups[1].Value));
}

// --- Step 2: Rewrite strings in .cs files ---
Console.WriteLine("Rewriting AddComponentMenu labels and editor resource names...");
var rewrittenCount = 0;
var rewriteHits = new int[editorRewrites.Length];
foreach (var csFile in Directory.EnumerateFiles(outputDir, "*.cs", SearchOption.AllDirectories))
{
    var original = File.ReadAllText(csFile);
    var updated = original;
    for (var i = 0; i < editorRewrites.Length; i++)
    {
        var (oldText, newText) = editorRewrites[i];
        rewriteHits[i] += CountOccurrences(updated, oldText);
        updated = updated.Replace(oldText, newText);
    }
    updated = updated
        .Replace($"({OldMenuLabel})\"", $"({NewMenuLabel})\"")
        .Replace($"\"{OldMenuLabel}/", $"\"{NewMenuLabel}/");
    if (updated != original)
    {
        File.WriteAllText(csFile, updated);
        rewrittenCount++;
    }
}
Console.WriteLine($"  {rewrittenCount} files updated");
for (var i = 0; i < editorRewrites.Length; i++)
{
    if (rewriteHits[i] > 0) continue;
    Console.Error.WriteLine($"Editor rewrite target not found (changed upstream?): {editorRewrites[i].Old}");
    return 1;
}

// --- Step 2b: Strip [VFInit] from hooks whose Harmony patches SPSNDMF already applies ---
Console.WriteLine("Stripping [VFInit] from shared Harmony hooks...");
var stripVfInit = LoadList(stripVfInitPath);
foreach (var rel in stripVfInit)
{
    var target = Path.Combine(outputDir, rel);
    if (!File.Exists(target))
    {
        Console.Error.WriteLine($"strip-vfinit.txt entry not found in output: {rel}");
        return 1;
    }
    var lines = File.ReadAllText(target).Split('\n').ToList();
    var hits = lines.Select((l, i) => (l, i)).Where(x => x.l.Trim() == "[VFInit]").Select(x => x.i).ToList();
    if (hits.Count != 1)
    {
        Console.Error.WriteLine($"Expected exactly one [VFInit] line in {rel}, found {hits.Count}");
        return 1;
    }
    lines.RemoveAt(hits[0]);
    File.WriteAllText(target, string.Join('\n', lines));
}
Console.WriteLine($"  {stripVfInit.Count} files updated");

// --- Step 2c: Tests (local verification only, never shipped) ---
if (args.Contains("--include-tests"))
{
    Console.WriteLine("Copying Tests...");
    CopyDirectory(Path.Combine(scriptDir, "Tests"), Path.Combine(outputDir, "Tests"));
    File.Copy(Path.Combine(scriptDir, "Tests.meta"), Path.Combine(outputDir, "Tests.meta"));
    var internalsPath = Path.Combine(outputRuntimeDir, "_InternalsVisibleTo.cs");
    var internalsText = File.ReadAllText(internalsPath);
    File.WriteAllText(internalsPath,
        internalsText + (internalsText.EndsWith("\n") ? "" : "\r\n") +
        "[assembly: InternalsVisibleTo(\"VRCFuryStub-Tests\")]\r\n");
}

// --- Step 3: Copy package.json template ---
Console.WriteLine("Copying package.json...");
var packageJsonTemplate = Path.Combine(scriptDir, "package.json");
if (!File.Exists(packageJsonTemplate))
{
    Console.Error.WriteLine($"package.json template not found at {packageJsonTemplate}");
    return 1;
}
File.Copy(packageJsonTemplate, Path.Combine(outputDir, "package.json"), overwrite: true);
File.Copy(packageJsonTemplate + ".meta", Path.Combine(outputDir, "package.json.meta"), overwrite: true);

// --- Step 4: Copy CHANGELOG.md template ---
var changelogTemplate = Path.Combine(scriptDir, "CHANGELOG.md");
if (File.Exists(changelogTemplate))
{
    Console.WriteLine("Copying CHANGELOG.md...");
    File.Copy(changelogTemplate, Path.Combine(outputDir, "CHANGELOG.md"), overwrite: true);
    File.Copy(changelogTemplate + ".meta", Path.Combine(outputDir, "CHANGELOG.md.meta"), overwrite: true);
}

CheckExcludedTypesUnreferenced(sourcePackageDir, outputDir, excludedFiles);

Console.WriteLine();
Console.WriteLine("Done.");
return 0;

// ============================================================

List<string> LoadList(string path) =>
    File.ReadAllLines(path)
        .Select(l => l.Trim())
        .Where(l => l.Length > 0 && !l.StartsWith("#"))
        .Select(l => l.Replace('\\', '/'))
        .ToList();

int CountOccurrences(string text, string token)
{
    var count = 0;
    for (var i = text.IndexOf(token, StringComparison.Ordinal); i >= 0; i = text.IndexOf(token, i + token.Length, StringComparison.Ordinal))
        count++;
    return count;
}

// Copies source/rel (+ .meta) to dest/rel, creating each parent folder with its upstream folder meta.
void CopyWithParentMetas(string source, string dest, string rel)
{
    var parts = rel.Split('/');
    var dir = "";
    for (var i = 0; i < parts.Length - 1; i++)
    {
        dir = i == 0 ? parts[0] : dir + "/" + parts[i];
        var outDir = Path.Combine(dest, dir);
        if (Directory.Exists(outDir)) continue;
        Directory.CreateDirectory(outDir);
        File.Copy(Path.Combine(source, dir + ".meta"), outDir + ".meta");
    }
    File.Copy(Path.Combine(source, rel), Path.Combine(dest, rel));
    File.Copy(Path.Combine(source, rel + ".meta"), Path.Combine(dest, rel + ".meta"));
}

// A shipped file that still names a type from an excluded file would only fail inside Unity,
// so catch it here. Top-level types only: nested helpers like "Reflection" appear in every hook.
void CheckExcludedTypesUnreferenced(string source, string dest, List<string> excluded)
{
    var typeDecl = new Regex(@"^ {0,4}(?:\[[^\]]*\] *)*(?:(?:internal|public|private|static|abstract|sealed|partial) )*(?:class|struct|enum|interface) +([A-Za-z_]\w*)", RegexOptions.Multiline);
    var excludedTypes = excluded
        .Where(rel => rel.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        .SelectMany(rel => typeDecl.Matches(File.ReadAllText(Path.Combine(source, rel))).Select(m => (Type: m.Groups[1].Value, File: rel)))
        .ToList();
    var failed = false;
    foreach (var cs in Directory.EnumerateFiles(dest, "*.cs", SearchOption.AllDirectories))
    {
        var text = File.ReadAllText(cs);
        foreach (var (type, file) in excludedTypes)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(type)}\b"))
            {
                Console.Error.WriteLine($"{Path.GetRelativePath(dest, cs)} references {type} from excluded {file}");
                failed = true;
            }
        }
    }
    if (failed) Environment.Exit(1);
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

void CopyDirectory(string source, string dest)
{
    Directory.CreateDirectory(dest);
    foreach (var file in Directory.GetFiles(source))
        File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
    foreach (var dir in Directory.GetDirectories(source))
        CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
}
