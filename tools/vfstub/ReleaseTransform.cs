using System.Text.RegularExpressions;

// --- Paths (relative to this script file) ---
static string GetScriptDir([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
    Path.GetDirectoryName(path)!;
var scriptDir = GetScriptDir();
var repoRoot = Path.GetFullPath(Path.Combine(scriptDir, "..", ".."));

var sourceRuntimeDir = Path.Combine(repoRoot, "com.vrcfury.vrcfury", "Runtime");
var outputDir = Path.Combine(repoRoot, "net.nrzk.vfstub");
var outputRuntimeDir = Path.Combine(outputDir, "Runtime");

// --- Configuration ---
const string OldAsmdefName = "VRCFury";
const string NewAsmdefName = "Nrzk.VFStub";

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

// --- Step 2: Rewrite asmdef ---
var sourceAsmdef = Path.Combine(outputRuntimeDir, OldAsmdefName + ".asmdef");
var sourceAsmdefMeta = sourceAsmdef + ".meta";
var targetAsmdef = Path.Combine(outputRuntimeDir, NewAsmdefName + ".asmdef");
var targetAsmdefMeta = targetAsmdef + ".meta";

if (!File.Exists(sourceAsmdef))
{
    Console.Error.WriteLine($"asmdef not found at {sourceAsmdef}");
    return 1;
}

Console.WriteLine("Rewriting asmdef name...");
var asmdefContent = File.ReadAllText(sourceAsmdef);
asmdefContent = Regex.Replace(
    asmdefContent,
    "\"name\"\\s*:\\s*\"" + Regex.Escape(OldAsmdefName) + "\"",
    $"\"name\": \"{NewAsmdefName}\"");
File.WriteAllText(sourceAsmdef, asmdefContent);

File.Move(sourceAsmdef, targetAsmdef);
if (File.Exists(sourceAsmdefMeta))
    File.Move(sourceAsmdefMeta, targetAsmdefMeta);

Console.WriteLine($"  {OldAsmdefName}.asmdef -> {NewAsmdefName}.asmdef");

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
