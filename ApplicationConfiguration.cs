namespace ProjectContextAnalyser
{
    internal class ApplicationConfiguration
    {
        public const int FolderOverviewThreshold = 100;
        
        public static readonly List<string> RecognizedProgrammingExtensions = new()
        {
            ".js",
            ".py",
            ".ts",
            ".java",
            ".cs",
            ".cpp",
            ".cc",
            ".cxx",
            ".c",
            ".php",
            ".go",
            ".rs",
            ".vb",
            ".bas",
            ".frm",
            ".cls"
        };

        public static readonly List<string> SupportedOutputExtensions = new()
        {
            ".txt",
            ".md"
        };

        public static readonly List<string> BinaryExclusionForContentAnalysis = new()
        {
            ".ppt",
            ".pptx",
            ".xls",
            ".xlsx",
            ".doc",
            ".docx",
            ".pdf",
            ".exe",
            ".dll",
            ".zip",
            ".7z",
            ".rar",
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".bmp",
            ".ico"
        };

        public const string DefaultOutputFileExtension = ".txt";

        public const OutputMode DefaultOutputMode = OutputMode.Full;

        public const IgnoreMode DefaultOutputIgnoreMode = IgnoreMode.Contents;
    }
}