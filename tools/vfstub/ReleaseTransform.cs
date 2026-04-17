// --- Paths (relative to this script file) ---
static string GetScriptDir([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
    Path.GetDirectoryName(path)!;
var scriptDir = GetScriptDir();
var repoRoot = Path.GetFullPath(Path.Combine(scriptDir, "..", ".."));

var sourceRuntimeDir = Path.Combine(repoRoot, "com.vrcfury.vrcfury", "Runtime");
var outputDir = Path.Combine(repoRoot, "net.nrzk.vfstub");
var outputRuntimeDir = Path.Combine(outputDir, "Runtime");

// --- Configuration ---
// asmdef name is kept as-is ("VRCFury") so [SerializeReference] type references
// in existing assets (which embed the assembly name) resolve without MovedFrom
// attributes. The stub is drop-in compatible and mutually exclusive with the
// real VRCFury package.
const string OldMenuLabel = "VRCFury";
const string NewMenuLabel = "VRCFuryStub";

// --- Validate ---
if (!Directory.Exists(sourceRuntimeDir))
{
    Console.Error.WriteLine($"Source Runtime directory not found: {sourceRuntimeDir}");
    return 1;
}

Console.WriteLine($"Repo root: {repoRoot}");
Console.WriteLine($"Source:    {sourceRuntimeDir}");
Console.WriteLine($"Output:    {outputDir}");

// --- Step 1: Clean and copy Runtime ---
Console.WriteLine("Copying Runtime...");
if (Directory.Exists(outputDir))
    Directory.Delete(outputDir, recursive: true);
Directory.CreateDirectory(outputDir);
CopyDirectory(sourceRuntimeDir, outputRuntimeDir);

// --- Step 2: Rewrite AddComponentMenu labels in .cs files ---
Console.WriteLine("Rewriting AddComponentMenu labels...");
var rewrittenCount = 0;
foreach (var csFile in Directory.EnumerateFiles(outputRuntimeDir, "*.cs", SearchOption.AllDirectories))
{
    var original = File.ReadAllText(csFile);
    var updated = original
        .Replace($"({OldMenuLabel})\"", $"({NewMenuLabel})\"")
        .Replace($"\"{OldMenuLabel}/", $"\"{NewMenuLabel}/");
    if (updated != original)
    {
        File.WriteAllText(csFile, updated);
        rewrittenCount++;
    }
}
Console.WriteLine($"  {rewrittenCount} files updated");

// --- Step 3: Copy package.json template ---
Console.WriteLine("Copying package.json...");
var packageJsonTemplate = Path.Combine(scriptDir, "package.json");
if (!File.Exists(packageJsonTemplate))
{
    Console.Error.WriteLine($"package.json template not found at {packageJsonTemplate}");
    return 1;
}
File.Copy(packageJsonTemplate, Path.Combine(outputDir, "package.json"), overwrite: true);

Console.WriteLine();
Console.WriteLine("Done.");
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
