using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldUnityBuild
    {
        private const string GateAScenePath = "Assets/Scenes/AshenThresholdSpike.unity";
        private const string D020SliceScenePath = FourfoldD020SliceSceneBuilder.ScenePath;
        private const string DefaultBuildRoot = "Build/GateA";
        private const string DefaultD020SliceBuildRoot = "Build/D020Slice";
        private const string DefaultProductBuildRoot = "Build/FourfoldEchoes";
        private const string ProductName = "FourfoldEchoesGateA";
        private const string D020SliceProductName = "FourfoldEchoesD020Slice";
        private const string FullProductName = "FourfoldEchoes";
        private const string CompanyName = "RollNuts";
        private const string DefaultBuildVersion = "0.1.0-dev";
        private const string DefaultCommitSha = "local-dev";
        private const string BuildInfoResourceFolder = "Assets/Resources";
        private const string BuildInfoResourceFolderMetaPath = BuildInfoResourceFolder + ".meta";
        private const string BuildInfoResourcePath = BuildInfoResourceFolder + "/FourfoldBuildInfo.txt";
        private const string BuildInfoResourceMetaPath = BuildInfoResourcePath + ".meta";

        public static void BuildCurrentProductLoop()
        {
            var target = GetRequestedTarget();
            var buildRoot = GetRequestedBuildRoot(DefaultProductBuildRoot);
            var artifactPath = GetArtifactPath(buildRoot, target, FullProductName);

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
            {
                throw new InvalidOperationException(
                    $"Unity standalone build target is not installed or supported in this editor: {target}");
            }

            FourfoldD020SliceSceneBuilder.BuildAndValidate();
            FourfoldHubSceneBuilder.BuildAndValidate();
            FourfoldTitleSceneBuilder.BuildAndValidate();
            BuildScenes(target, artifactPath, new[] { FourfoldTitleSceneBuilder.ScenePath, FourfoldHubSceneBuilder.ScenePath, D020SliceScenePath }, FullProductName, "Title+Hub+D020 product loop");
        }

        public static void BuildCurrentD020Slice()
        {
            var target = GetRequestedTarget();
            var buildRoot = GetRequestedBuildRoot(DefaultD020SliceBuildRoot);
            var artifactPath = GetArtifactPath(buildRoot, target, D020SliceProductName);

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
            {
                throw new InvalidOperationException(
                    $"Unity standalone build target is not installed or supported in this editor: {target}");
            }

            FourfoldD020SliceSceneBuilder.BuildAndValidate();
            BuildScenes(target, artifactPath, new[] { D020SliceScenePath }, D020SliceProductName, "D-020 vertical slice");
        }

        public static void BuildGateA()
        {
            var target = GetRequestedTarget();
            var buildRoot = GetRequestedBuildRoot();
            BuildGateA(target, GetArtifactPath(buildRoot, target));
        }

        public static void BuildGateAMacOS()
        {
            var buildRoot = GetRequestedBuildRoot();
            BuildGateA(BuildTarget.StandaloneOSX, GetArtifactPath(buildRoot, BuildTarget.StandaloneOSX));
        }

        public static void BuildGateAWindows()
        {
            var buildRoot = GetRequestedBuildRoot();
            BuildGateA(BuildTarget.StandaloneWindows64, GetArtifactPath(buildRoot, BuildTarget.StandaloneWindows64));
        }

        private static void BuildGateA(BuildTarget target, string artifactPath)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
            {
                throw new InvalidOperationException(
                    $"Unity standalone build target is not installed or supported in this editor: {target}");
            }

            FourfoldUnitySpikeBuilder.BuildAndValidate();

            if (!File.Exists(GateAScenePath))
            {
                throw new FileNotFoundException("Gate A scene was not generated before build.", GateAScenePath);
            }

            BuildScenes(target, artifactPath, new[] { GateAScenePath }, ProductName, "Gate A");
        }

        private static BuildTarget GetRequestedTarget()
        {
            var value = GetArgument("--fourfoldBuildTarget");
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable("FOURFOLD_BUILD_TARGET");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                value = "macos";
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "macos":
                case "osx":
                case "standaloneosx":
                    return BuildTarget.StandaloneOSX;

                case "windows":
                case "win64":
                case "standalonewindows64":
                    return BuildTarget.StandaloneWindows64;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported FOURFOLD build target '{value}'. Expected macos or windows.");
            }
        }

        private static string GetRequestedBuildRoot(string defaultBuildRoot = DefaultBuildRoot)
        {
            var value = GetArgument("--fourfoldBuildDir");
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable("FOURFOLD_BUILD_DIR");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultBuildRoot;
            }

            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.IsPathRooted(value) ? value : Path.Combine(repoRoot, value));
        }

        private static string GetArtifactPath(string buildRoot, BuildTarget target)
        {
            return GetArtifactPath(buildRoot, target, ProductName);
        }

        private static string GetArtifactPath(string buildRoot, BuildTarget target, string productName)
        {
            switch (target)
            {
                case BuildTarget.StandaloneOSX:
                    return Path.Combine(buildRoot, "macos", productName + ".app");

                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(buildRoot, "windows", productName + ".exe");

                default:
                    throw new InvalidOperationException($"Unsupported Gate A build target: {target}");
            }
        }

        private static void BuildScenes(BuildTarget target, string artifactPath, string[] scenePaths, string productName, string label)
        {
            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            if (!string.IsNullOrWhiteSpace(artifactDirectory))
            {
                Directory.CreateDirectory(artifactDirectory);
            }

            var originalProductName = PlayerSettings.productName;
            var originalCompanyName = PlayerSettings.companyName;
            var originalBundleVersion = PlayerSettings.bundleVersion;
            var buildVersion = GetRequestedBuildVersion(originalBundleVersion);
            var commitSha = GetRequestedCommitSha();
            BuildInfoBackup buildInfoBackup = null;
            BuildReport report;
            try
            {
                PlayerSettings.productName = productName;
                PlayerSettings.companyName = CompanyName;
                PlayerSettings.bundleVersion = buildVersion;
                buildInfoBackup = WriteBuildInfoAsset(productName, buildVersion, commitSha, target, label);

                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenePaths,
                    locationPathName = artifactPath,
                    target = target,
                    options = BuildOptions.None
                });
            }
            finally
            {
                PlayerSettings.productName = originalProductName;
                PlayerSettings.companyName = originalCompanyName;
                PlayerSettings.bundleVersion = originalBundleVersion;
                RestoreBuildInfoAsset(buildInfoBackup);
            }

            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"FOURFOLD {label} build failed: target={target} result={summary.result} errors={summary.totalErrors}");
            }

            if (!ArtifactExists(artifactPath))
            {
                throw new FileNotFoundException("Unity reported success, but the build artifact was not found.", artifactPath);
            }

            Debug.Log(
                $"FOURFOLD {label} build succeeded: target={target} artifact={Path.GetFullPath(artifactPath)} sizeBytes={CalculateSizeBytes(artifactPath)}");
        }

        private static string GetRequestedBuildVersion(string originalBundleVersion)
        {
            var value = GetArgument("--fourfoldBuildVersion");
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable("FOURFOLD_BUILD_VERSION");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                value = originalBundleVersion;
            }

            return string.IsNullOrWhiteSpace(value) ? DefaultBuildVersion : CleanBuildInfoValue(value);
        }

        private static string GetRequestedCommitSha()
        {
            var value = GetArgument("--fourfoldCommitSha");
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable("FOURFOLD_COMMIT_SHA");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                value = ReadGitCommitSha();
            }

            return string.IsNullOrWhiteSpace(value) ? DefaultCommitSha : CleanBuildInfoValue(value);
        }

        private static string ReadGitCommitSha()
        {
            try
            {
                var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var startInfo = new System.Diagnostics.ProcessStartInfo("git", "rev-parse --short=12 HEAD")
                {
                    WorkingDirectory = repoRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return DefaultCommitSha;
                    }

                    if (!process.WaitForExit(3000))
                    {
                        process.Kill();
                        return DefaultCommitSha;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    return process.ExitCode == 0 ? output.Trim() : DefaultCommitSha;
                }
            }
            catch
            {
                return DefaultCommitSha;
            }
        }

        private static BuildInfoBackup WriteBuildInfoAsset(string productName, string version, string commitSha, BuildTarget target, string label)
        {
            var backup = BuildInfoBackup.Capture();
            Directory.CreateDirectory(BuildInfoResourceFolder);
            File.WriteAllText(
                BuildInfoResourcePath,
                $"product={CleanBuildInfoValue(productName)}\nversion={CleanBuildInfoValue(version)}\ncommit={CleanBuildInfoValue(commitSha)}\ntarget={CleanBuildInfoValue(target.ToString())}\nlabel={CleanBuildInfoValue(label)}\n");
            AssetDatabase.ImportAsset(BuildInfoResourcePath, ImportAssetOptions.ForceUpdate);
            return backup;
        }

        private static void RestoreBuildInfoAsset(BuildInfoBackup backup)
        {
            if (backup == null)
            {
                return;
            }

            RestoreFile(BuildInfoResourcePath, backup.FileExisted, backup.FileText);
            RestoreFile(BuildInfoResourceMetaPath, backup.FileMetaExisted, backup.FileMetaText);
            RestoreFile(BuildInfoResourceFolderMetaPath, backup.FolderMetaExisted, backup.FolderMetaText);

            if (!backup.FolderExisted && Directory.Exists(BuildInfoResourceFolder) && Directory.GetFileSystemEntries(BuildInfoResourceFolder).Length == 0)
            {
                Directory.Delete(BuildInfoResourceFolder);
            }

            AssetDatabase.Refresh();
        }

        private static void RestoreFile(string path, bool existed, string text)
        {
            if (existed)
            {
                File.WriteAllText(path, text);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string CleanBuildInfoValue(string value)
        {
            return (value ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        private static bool ArtifactExists(string artifactPath)
        {
            return File.Exists(artifactPath) || Directory.Exists(artifactPath);
        }

        private static long CalculateSizeBytes(string artifactPath)
        {
            if (File.Exists(artifactPath))
            {
                return new FileInfo(artifactPath).Length;
            }

            var total = 0L;
            foreach (var file in Directory.EnumerateFiles(artifactPath, "*", SearchOption.AllDirectories))
            {
                total += new FileInfo(file).Length;
            }

            return total;
        }

        private static string GetArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length - 1; index++)
            {
                if (args[index] == name)
                {
                    return args[index + 1];
                }
            }

            return null;
        }

        private sealed class BuildInfoBackup
        {
            public bool FolderExisted;
            public bool FolderMetaExisted;
            public bool FileExisted;
            public bool FileMetaExisted;
            public string FolderMetaText;
            public string FileText;
            public string FileMetaText;

            public static BuildInfoBackup Capture()
            {
                return new BuildInfoBackup
                {
                    FolderExisted = Directory.Exists(BuildInfoResourceFolder),
                    FolderMetaExisted = File.Exists(BuildInfoResourceFolderMetaPath),
                    FileExisted = File.Exists(BuildInfoResourcePath),
                    FileMetaExisted = File.Exists(BuildInfoResourceMetaPath),
                    FolderMetaText = ReadIfExists(BuildInfoResourceFolderMetaPath),
                    FileText = ReadIfExists(BuildInfoResourcePath),
                    FileMetaText = ReadIfExists(BuildInfoResourceMetaPath)
                };
            }

            private static string ReadIfExists(string path)
            {
                return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            }
        }
    }
}
