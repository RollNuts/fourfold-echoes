namespace FourfoldEchoes.Product
{
    public static class FourfoldLanguage
    {
        public const string English = "en";
        public const string Japanese = "ja";

        public static string Sanitize(string value)
        {
            return value == Japanese ? Japanese : English;
        }

        public static bool IsJapanese(FourfoldProgressData progressData)
        {
            return progressData != null && Sanitize(progressData.language) == Japanese;
        }

        public static string Label(FourfoldProgressData progressData)
        {
            return IsJapanese(progressData) ? "日本語" : "English";
        }

        public static string Toggle(string value)
        {
            return Sanitize(value) == Japanese ? English : Japanese;
        }

        public static string T(FourfoldProgressData progressData, string english, string japanese)
        {
            return IsJapanese(progressData) ? japanese : english;
        }
    }
}
