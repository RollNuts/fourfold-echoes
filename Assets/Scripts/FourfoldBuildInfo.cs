using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public static class FourfoldBuildInfo
    {
        private const string ResourceName = "FourfoldBuildInfo";
        private const string DefaultVersion = "0.1.0-dev";
        private const string DefaultCommit = "local-dev";
        private static string cachedBuildInfoText;

        public static string BuildVersion
        {
            get
            {
                var version = ReadField("version", string.Empty);
                if (string.IsNullOrWhiteSpace(version))
                {
                    version = Application.version;
                }

                return string.IsNullOrWhiteSpace(version) ? DefaultVersion : version.Trim();
            }
        }

        public static string CommitSha
        {
            get
            {
                var commit = ReadField("commit", string.Empty);
                return string.IsNullOrWhiteSpace(commit) ? DefaultCommit : commit.Trim();
            }
        }

        public static string ShortCommitSha
        {
            get
            {
                var commit = CommitSha;
                return commit.Length > 12 ? commit.Substring(0, 12) : commit;
            }
        }

        public static string TitleBuildLine(FourfoldProgressData data)
        {
            return FourfoldLanguage.T(
                data,
                $"Build {BuildVersion} / commit {ShortCommitSha}",
                $"Build {BuildVersion} / commit {ShortCommitSha}");
        }

        private static string ReadField(string key, string fallback)
        {
            var text = BuildInfoText;
            if (string.IsNullOrWhiteSpace(text))
            {
                return fallback;
            }

            var prefix = key + "=";
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return line.Substring(prefix.Length).Trim();
                }
            }

            return fallback;
        }

        private static string BuildInfoText
        {
            get
            {
                if (cachedBuildInfoText != null)
                {
                    return cachedBuildInfoText;
                }

                var asset = Resources.Load<TextAsset>(ResourceName);
                cachedBuildInfoText = asset == null ? string.Empty : asset.text;
                return cachedBuildInfoText;
            }
        }
    }
}
