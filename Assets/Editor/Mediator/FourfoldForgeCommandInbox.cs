using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor.Mediator
{
    [InitializeOnLoad]
    public static class FourfoldForgeCommandInbox
    {
        private const double PollIntervalSeconds = 0.75d;
        private static double nextPollTime;
        private static bool isProcessing;

        static FourfoldForgeCommandInbox()
        {
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (isProcessing || EditorApplication.timeSinceStartup < nextPollTime)
            {
                return;
            }

            nextPollTime = EditorApplication.timeSinceStartup + PollIntervalSeconds;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var inbox = InboxPaths.FromProject();
            Directory.CreateDirectory(inbox.Commands);
            Directory.CreateDirectory(inbox.Events);
            Directory.CreateDirectory(inbox.Processed);
            Directory.CreateDirectory(inbox.Failed);

            var files = Directory.GetFiles(inbox.Commands, "*.ready.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.Ordinal);

            if (files.Length == 0)
            {
                return;
            }

            isProcessing = true;
            try
            {
                ProcessFile(inbox, files[0]);
            }
            finally
            {
                isProcessing = false;
            }
        }

        private static void ProcessFile(InboxPaths inbox, string path)
        {
            var commandId = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
            var result = new InboxResult
            {
                commandId = commandId,
                action = string.Empty,
                ok = false,
                code = "E_INTERNAL",
                message = string.Empty,
                artifacts = new string[0]
            };

            try
            {
                var command = JsonUtility.FromJson<InboxCommand>(File.ReadAllText(path));
                if (command == null || string.IsNullOrWhiteSpace(command.action))
                {
                    throw new InvalidOperationException("Command action is required.");
                }

                if (!string.IsNullOrWhiteSpace(command.commandId))
                {
                    commandId = command.commandId;
                    result.commandId = command.commandId;
                }

                result.action = command.action;
                Execute(command);

                result.ok = true;
                result.code = "OK";
                result.message = "Command completed.";
                File.WriteAllText(Path.Combine(inbox.Events, SafeFileName(commandId) + ".json"), JsonUtility.ToJson(result, true));
                MoveCommand(path, Path.Combine(inbox.Processed, Path.GetFileName(path)));
            }
            catch (Exception error)
            {
                result.ok = false;
                result.code = "E_COMMAND_FAILED";
                result.message = error.Message;
                File.WriteAllText(Path.Combine(inbox.Events, SafeFileName(commandId) + ".json"), JsonUtility.ToJson(result, true));
                MoveCommand(path, Path.Combine(inbox.Failed, Path.GetFileName(path)));
                Debug.LogError($"FOURFOLD Forge inbox command failed: {error}");
            }
        }

        private static void Execute(InboxCommand command)
        {
            switch (command.action)
            {
                case "d020.build_and_validate":
                    FourfoldD020SliceSceneBuilder.BuildAndValidate();
                    break;

                case "d020.validate":
                    FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();
                    break;

                case "d020.capture_evidence":
                    FourfoldUnityEvidenceCapture.CaptureD020Slice();
                    break;

                case "d021.contract_validate":
                    FourfoldD021ProductContractVerifier.VerifyD021Contract();
                    break;

                case "product.validate":
                    FourfoldProductValidator.RunAll();
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported command action: {command.action}");
            }
        }

        private static void MoveCommand(string source, string destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? ".");
            var resolved = destination;
            var attempt = 1;
            while (File.Exists(resolved))
            {
                resolved = Path.Combine(
                    Path.GetDirectoryName(destination) ?? ".",
                    Path.GetFileNameWithoutExtension(destination) + $".{attempt}" + Path.GetExtension(destination));
                attempt++;
            }

            File.Move(source, resolved);
        }

        private static string SafeFileName(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value;
        }

        [Serializable]
        private sealed class InboxCommand
        {
            public string commandId;
            public string action;
        }

        [Serializable]
        private sealed class InboxResult
        {
            public string commandId;
            public string action;
            public bool ok;
            public string code;
            public string message;
            public string[] artifacts;
        }

        private readonly struct InboxPaths
        {
            public readonly string Commands;
            public readonly string Events;
            public readonly string Processed;
            public readonly string Failed;

            private InboxPaths(string root)
            {
                Commands = Path.Combine(root, "commands");
                Events = Path.Combine(root, "events");
                Processed = Path.Combine(root, "processed");
                Failed = Path.Combine(root, "failed");
            }

            public static InboxPaths FromProject()
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                return new InboxPaths(Path.Combine(projectRoot, "Temp", "FourfoldForgeInbox"));
            }
        }
    }
}
