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
        private const string DefaultBuildRoot = "Build/GateA";
        private const string ProductName = "FourfoldEchoesGateA";

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

            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            if (!string.IsNullOrWhiteSpace(artifactDirectory))
            {
                Directory.CreateDirectory(artifactDirectory);
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { GateAScenePath },
                locationPathName = artifactPath,
                target = target,
                options = BuildOptions.None
            });

            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"FOURFOLD Gate A build failed: target={target} result={summary.result} errors={summary.totalErrors}");
            }

            if (!ArtifactExists(artifactPath))
            {
                throw new FileNotFoundException("Unity reported success, but the build artifact was not found.", artifactPath);
            }

            Debug.Log(
                $"FOURFOLD Gate A build succeeded: target={target} artifact={Path.GetFullPath(artifactPath)} sizeBytes={CalculateSizeBytes(artifactPath)}");
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

        private static string GetRequestedBuildRoot()
        {
            var value = GetArgument("--fourfoldBuildDir");
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable("FOURFOLD_BUILD_DIR");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                value = DefaultBuildRoot;
            }

            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.IsPathRooted(value) ? value : Path.Combine(repoRoot, value));
        }

        private static string GetArtifactPath(string buildRoot, BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneOSX:
                    return Path.Combine(buildRoot, "macos", ProductName + ".app");

                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(buildRoot, "windows", ProductName + ".exe");

                default:
                    throw new InvalidOperationException($"Unsupported Gate A build target: {target}");
            }
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
    }
}
