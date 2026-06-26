using System;
using System.IO;
using FourfoldEchoes.Editor;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor.Mediator
{
    public static class FourfoldForgeMediator
    {
        private const string DefaultCommandPath = "commands/samples/inspect-d020-slice.json";

        public static void Run()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var commandPath = GetArgument("--commandFile") ?? DefaultCommandPath;
            var commandFullPath = ResolveInsideRepo(repoRoot, commandPath);
            var command = JsonUtility.FromJson<ForgeCommand>(File.ReadAllText(commandFullPath));

            if (string.IsNullOrWhiteSpace(command.commandId))
            {
                throw new InvalidOperationException("Forge command is missing commandId.");
            }

            try
            {
                Execute(command);
                WriteEvent(repoRoot, command.commandId, "forge.command.complete", "ok", command.sceneId, "Command completed.", new string[0]);
            }
            catch (Exception error)
            {
                WriteEvent(repoRoot, command.commandId, "forge.command.failed", "error", command.sceneId, error.Message, new string[0]);
                throw;
            }
        }

        private static void Execute(ForgeCommand command)
        {
            switch (command.action)
            {
                case "inspect_project":
                case "inspect_scene":
                case "plan_operation":
                    Debug.Log($"Forge command accepted without scene mutation: {command.action}");
                    break;

                case "run_room_spike":
                    FourfoldUnitySpikeBuilder.BuildAndValidate();
                    break;

                case "d020.build_and_validate":
                    FourfoldD020SliceSceneBuilder.BuildAndValidate();
                    break;

                case "d020.validate":
                    FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();
                    break;

                case "d020.capture_evidence":
                    FourfoldUnityEvidenceCapture.CaptureD020Slice();
                    break;

                case "capture_scene":
                    FourfoldUnitySpikeBuilder.BuildAndValidate();
                    Debug.Log("Forge capture requested. Screenshot capture is deferred until the capture camera runner lands.");
                    break;

                case "run_playmode_tests":
                    Debug.Log("Forge playmode test command accepted. Test Framework execution is handled by Unity CLI -runTests.");
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported Forge command action: {command.action}");
            }
        }

        private static string ResolveInsideRepo(string repoRoot, string relativeOrAbsolutePath)
        {
            var fullPath = Path.GetFullPath(Path.IsPathRooted(relativeOrAbsolutePath)
                ? relativeOrAbsolutePath
                : Path.Combine(repoRoot, relativeOrAbsolutePath));
            var rootWithSeparator = repoRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? repoRoot
                : repoRoot + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Forge command path must stay inside the repository.");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Forge command file was not found.", fullPath);
            }

            return fullPath;
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

        private static void WriteEvent(string repoRoot, string commandId, string eventName, string status, string semanticId, string message, string[] artifacts)
        {
            var eventsFolder = Path.Combine(repoRoot, "events");
            Directory.CreateDirectory(eventsFolder);
            var eventPath = Path.Combine(eventsFolder, $"{SafeFileName(commandId)}.jsonl");
            var json = "{"
                + $"\"event\":\"{Escape(eventName)}\","
                + $"\"commandId\":\"{Escape(commandId)}\","
                + $"\"semanticId\":\"{Escape(semanticId ?? "scene.d020_vertical_slice")}\","
                + $"\"status\":\"{Escape(status)}\","
                + $"\"message\":\"{Escape(message)}\","
                + "\"artifacts\":["
                + string.Join(",", Array.ConvertAll(artifacts, item => $"\"{Escape(item)}\""))
                + "]"
                + "}";
            File.AppendAllText(eventPath, json + Environment.NewLine);
            AssetDatabase.Refresh();
            Debug.Log($"Forge event written: {eventPath}");
        }

        private static string SafeFileName(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }

        [Serializable]
        public sealed class ForgeCommand
        {
            public string commandId;
            public string action;
            public string sceneId;
            public int seed;
            public ForgeEvidence evidence;
        }

        [Serializable]
        public sealed class ForgeEvidence
        {
            public bool screenshot;
            public bool video;
            public bool playmodeLog;
        }
    }
}
