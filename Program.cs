using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            RetrievedInputInformation info = GetConfigurationFromInputs(args);

            // validation
            bool validationIndicatesError = ConfigurationValidation.Validate(info);

            if (validationIndicatesError)
            {
                return;
            }

            JsonConfiguration configuration = info.Config!;

            

            configuration = ConfigurationPreprocessing.PreProcess(configuration); // Validation above guarantees configuration is not null here.

            List<ProjectFile> projectFiles = GetProjectFileInformation(configuration);

            OrchestratorFullRequestedAnalysis.Run(projectFiles, configuration);



           // Console.WriteLine($"Configuration file: {configFileName}");

            // Console.WriteLine($"Project folder: {configuration.ProjectFolderPath}");

        }

        static RetrievedInputInformation GetConfigurationFromInputs(string[] args)
        {
            RetrievedInputInformation info = new();
            
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a JSON configuration file.");
                info.Config = null;
                return info;
            }

            string configFileName = args[0];
            
            info.ConfigFileName =  configFileName;

            string json = File.ReadAllText(info.ConfigFileName);

            JsonConfiguration? configuration = JsonSerializer.Deserialize<JsonConfiguration>(json);

            info.Config = configuration;
            
            return info;
        }
        
        static List<ProjectFile> GetProjectFileInformation(JsonConfiguration configuration)
        {
            string projectFolderPath = configuration.ProjectFolderPath;

            string[] files = Directory.GetFiles(projectFolderPath, "*", SearchOption.AllDirectories);

            List<ProjectFile> projectFiles = new();
            
            foreach (var file in files)
            {
                ProjectFile projectFile = new();

                projectFile.FullName = file;

                string? thisPath = Path.GetDirectoryName(file);
                if (thisPath != null)
                {
                    projectFile.Path = Path.GetRelativePath(
                        configuration.ProjectFolderPath,
                        thisPath
                        ) + Path.DirectorySeparatorChar;
                }

                projectFile.NameOnlyWithExtension = Path.GetFileName(file);
                projectFile.NameOnlyWithoutExtension = Path.GetFileNameWithoutExtension(file);

                projectFile.Extension = Path.GetExtension(file);

                projectFiles.Add(projectFile);
            }

            return projectFiles;
        }

    }

    static class Normalisation
    {
        public static string NormaliseIgnorePath(string input)
        {
            string returnString = GeneralPathNormalise(input);

            if (!returnString.EndsWith(Path.DirectorySeparatorChar))
            {
                returnString += Path.DirectorySeparatorChar;
            }

            return returnString;
        }

        public static string GeneralPathNormalise(string input)
        {
            string returnString;
            returnString = Regex.Replace(input, @"\\+","\\");
            returnString = Regex.Replace(returnString, @"/+", "/");

            returnString = returnString.Replace('\\', Path.DirectorySeparatorChar);
            returnString = returnString.Replace('/', Path.DirectorySeparatorChar);
            return returnString;
        }
    }

    static class GeneralAnalysisHelpers
    {
        public static List<ProjectFile> FilterIgnoredProjectFiles(List<ProjectFile> projectFiles, ProcessedJsonConfiguration config)
        {
            List<ProjectFile> filteredOutput = new();

            foreach (var projectFile in projectFiles)
            {
                bool ignoreProjectFile = false;

                //Console.WriteLine($"projectFile = {projectFile.Path}");
                
                // filter based on ignored directories
                foreach (var ignoreDirectory in config.ProcIgnoredDirectories)
                {
                    //Console.WriteLine($"ignoreDirectory = {ignoreDirectory}");
                    if (projectFile.Path == ignoreDirectory || projectFile.Path.StartsWith(ignoreDirectory))
                    {
                        ignoreProjectFile = true;
                    }   
                }

                // filter based on ignored files
                foreach (var ignoreFile in config.ProcIgnoredFiles)
                {
                    if (!ignoreFile.IgnoredFileHasPath)
                    {
                        if (projectFile.NameOnlyWithExtension == ignoreFile.FullName)
                        {
                            ignoreProjectFile = true;
                        }
                    }
                    else
                    {
                        if (projectFile.FullName == ignoreFile.FullName)
                        {
                            ignoreProjectFile = true;
                        }
                    }
                }

                if (ignoreProjectFile)
                {
                    continue;
                }

                filteredOutput.Add(projectFile);
            }

            return filteredOutput;
        }

        public static List<ProjectFile> FilterIfApplicable(List<ProjectFile> projectFiles, ProcessedJsonConfiguration config, AnalysisType analysisType)
        {
            return 
                ShouldApplyIgnoreRules(config, analysisType)
                    ? GeneralAnalysisHelpers.FilterIgnoredProjectFiles(projectFiles, config)
                    : projectFiles;
        }

        public static bool ShouldApplyIgnoreRules(ProcessedJsonConfiguration config, AnalysisType analysisType)
        {
            if (analysisType == AnalysisType.Structure)
            {
                return config.ProcIgnoreMode == IgnoreMode.Structure || config.ProcIgnoreMode == IgnoreMode.Both;
            }

            if (analysisType == AnalysisType.Contents)
            {
                return config.ProcIgnoreMode == IgnoreMode.Contents || config.ProcIgnoreMode == IgnoreMode.Both;
            }
            
            Console.WriteLine("No valid AnalysisType inputted. Default to no filtering.");
            return false;
        }
    }

    static class OrchestratorFullRequestedAnalysis
    {
        public static void Run(List<ProjectFile> projectFiles, JsonConfiguration config)
        {
            
            StringBuilder sb = new();

            var procConfig = config.ProcessedConfig;

            var orderedFiles = 
                projectFiles
                            .OrderBy(f => f.Path)
                            .ThenBy(f => f.NameOnlyWithExtension)
                            .ToList();

            if (procConfig.RunStructure)
            {
                var orderedProjectFilesToUse = 
                    procConfig.IgnoreInStructure
                        ? GeneralAnalysisHelpers.FilterIgnoredProjectFiles(orderedFiles, procConfig)
                        : projectFiles;
                    

                StructureAnalysis.Run(sb, orderedProjectFilesToUse, config);
            }

            if (procConfig.RunContent)
            {
                var orderedProjectFilesToUse =
                    procConfig.IgnoreInContent
                        ? GeneralAnalysisHelpers.FilterIgnoredProjectFiles(orderedFiles, procConfig)
                        : projectFiles;

                ContentAnalysis.Run(sb, orderedProjectFilesToUse, config);
            }

            WriteProjectContextFile(sb, config.Output!.OutputFileName); // elders wordt gevalideerd dat Output niet null is, en anders een error gegeven, vandaar !. 
        }

        private static void WriteProjectContextFile(StringBuilder sb, string? fileName)
        {
            string outputFile =
                String.IsNullOrWhiteSpace(fileName) 
                    ? "Analysis.txt"
                    : fileName;

            File.WriteAllText(outputFile, sb.ToString());
        }

    }

    static class StructureAnalysis
    {
        public static void Run(StringBuilder sb, List<ProjectFile> orderedProjectFiles, JsonConfiguration config)
        {

            //Inplement structure analysis here on theseProjectFiles
            sb.AppendLine("Structure analysis:");
            sb.AppendLine();

            if (orderedProjectFiles.Count > ApplicationConfiguration.FolderOverviewThreshold)
            {
                AddFolderOverview(sb, orderedProjectFiles);
            }

            sb.AppendLine("Files:");
            sb.AppendLine();

            string? previousPath = null;
            int folderCount = 0;

            for (int fileIndex = 0; fileIndex < orderedProjectFiles.Count; fileIndex++)
            {
                var thisFile = orderedProjectFiles[fileIndex];

                if (thisFile.Path != previousPath)
                {
                    if (!String.IsNullOrEmpty(previousPath))
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    
                    sb.AppendLine($"Folder {thisFile.Path}:");
                    sb.AppendLine();
                    previousPath = thisFile.Path;
                    folderCount++;
                }

                sb.AppendLine($"- {thisFile.NameOnlyWithExtension}");
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"Summary stats: {orderedProjectFiles.Count} files distributed over {folderCount} folders.");
        }

        private static void AddFolderOverview(StringBuilder sb, List<ProjectFile> orderedProjectFiles)
        {
            string? previousPath = null;
            sb.AppendLine("Folder overview:");

            for (int fileIndex = 0; fileIndex < orderedProjectFiles.Count; fileIndex++)
            {
                var thisFile = orderedProjectFiles[fileIndex];

                if (thisFile.Path != previousPath)
                {
                    sb.AppendLine($"- {thisFile.Path}");
                    previousPath = thisFile.Path;
                }
            }

            sb.AppendLine();
        }
    }

    static class ContentAnalysis
    {
        public static void Run(StringBuilder sb, List<ProjectFile> orderedProjectFiles, JsonConfiguration config)
        { 
            //To do:
            // - add separations between files
            // - optimise catch part of try, catch control flow
            
            for (int fileIndex = 0; fileIndex < orderedProjectFiles.Count; fileIndex++)
            {
                var thisFile = orderedProjectFiles[fileIndex];

                bool extensionNotIncluded = !config.IncludedExtensions!.Contains(thisFile.Extension);
                bool extensionBinary = ApplicationConfiguration.BinaryExclusionForContentAnalysis.Contains(thisFile.Extension);
                
                sb.AppendLine($"File {thisFile.NameOnlyWithExtension}:");

                if (extensionBinary || extensionNotIncluded)
                {
                    sb.AppendLine($"Content not printed, since this file has the extension {thisFile.Extension}, which:");
                    if (extensionBinary)
                    {
                        sb.AppendLine("- in BinaryExclusionForContentAnalysis, in Application.Configuration.cs");
                    }
                    
                    if (extensionNotIncluded)
                    {
                        sb.AppendLine("- is not in the IncludedExtensions of the input json");
                    }
                    continue;
                }

                try
                {
                    string content = File.ReadAllText(thisFile.FullName);
                    sb.AppendLine();
                    sb.Append(content);
                    sb.AppendLine();

                }
                catch
                {
                    sb.AppendLine($"Could not read file {thisFile.NameOnlyWithExtension}, so its content was skipped.");
                    Console.WriteLine($"Could not read file {thisFile.NameOnlyWithExtension}, so its content was skipped.");
                    continue;
                }
            }
        }
    }
}