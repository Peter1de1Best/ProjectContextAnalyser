namespace ProjectContextAnalyser
{
    
    internal class RetrievedInputInformation
    {
        public JsonConfiguration? Config { get; set; }
        public string ConfigFileName { get; set; } = "";
    }

    internal class JsonConfiguration
    {
        public string ProjectFolderPath { get; set; } = "";
        public bool IsProgrammingProject { get; set; } = false;
        public List<string>? IncludedExtensions { get; set; }
        public List<string> IgnoredDirectories { get; set; } = new();
        public List<string> IgnoredFiles { get; set; } = new();
        public OutputConfiguration? Output { get; set; }
        public ProcessedJsonConfiguration ProcessedConfig {get; set;} = new();
    }


    
    internal class OutputConfiguration
    {
        public string? Mode { get; set; }
        public string? IgnoreMode { get; set; }
        public string? OutputFileName { get; set; }
    }

    internal class ProcessedJsonConfiguration
    {
        public OutputMode ProcMode { get; set; }
        public IgnoreMode ProcIgnoreMode { get; set; }
        public String ProcOutputFileName { get; set; } = "";
        public List<string> ProcIgnoredDirectories { get; set; } = new();
        public List<IgnoredFile> ProcIgnoredFiles { get; set; } = new();

        public bool RunStructure => ProcMode == OutputMode.Full || ProcMode == OutputMode.StructureOnly;
        public bool IgnoreInStructure => ProcIgnoreMode == IgnoreMode.Structure || ProcIgnoreMode == IgnoreMode.Both;

        public bool RunContent => ProcMode == OutputMode.Full;
        public bool IgnoreInContent => ProcIgnoreMode == IgnoreMode.Contents || ProcIgnoreMode == IgnoreMode.Both;

        
    }



    internal enum OutputMode
    {
        Full,
        StructureOnly
    }

    internal enum IgnoreMode
    {
        None,
        Structure,
        Contents,
        Both
    }

    internal enum AnalysisType
    {
        Structure,
        Contents
    }

    internal class ProjectFile
    {
        public string FullName { get; set; } = "";
        public string Path { get; set; } = "";
        public string NameOnlyWithExtension { get; set; } = "";
        public string NameOnlyWithoutExtension { get; set; } = "";
        public string Extension { get; set; } = "";
    }

    internal class IgnoredFile
    {
        public string FullName { get; set; } = "";
        public string Path { get; set; } = "";
        public string NameOnlyWithExtension { get; set; } = "";
        public bool IgnoredFileHasPath { get; set; }
    }
}