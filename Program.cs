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
            bool validationIndicatesError = ValidateInputConfiguration(info);

            if (validationIndicatesError)
            {
                return;
            }

            JsonConfiguration configuration = info.Config!;

            RunWarnings(configuration); // Validation above guarantees configuration is not null here.

            configuration = PreProcessJsonConfiguration(configuration); // Validation above guarantees configuration is not null here.

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

        static bool ValidateInputConfiguration(RetrievedInputInformation inputInfo)
        {
            JsonConfiguration? configuration = inputInfo.Config;

            bool validationIndicatesError = false;

            if (configuration == null)
            {
                Console.WriteLine("Configuration could not be loaded");
                return true;
            }

            // validate fields
            // ProjectFolderPath:
            validationIndicatesError = ValidateInputConfigProjFoldPath(configuration, validationIndicatesError, inputInfo.ConfigFileName);

            // IsProgrammingProject:
            validationIndicatesError = ValidateInputConfigIsProgrammingProject(configuration, validationIndicatesError);

            //IncludedExtensions:
            validationIndicatesError = ValidateInputConfigInclExt(configuration, validationIndicatesError);

            //IgnoredDirectories (method does not do anything):
            validationIndicatesError = ValidateInputConfigIgnDir(configuration, validationIndicatesError);

            //IgnoredFiles (method does not do anything):
            validationIndicatesError = ValidateInputConfigIgnFil(configuration, validationIndicatesError);

            // Output:
            validationIndicatesError = ValidateInputConfigOutput(configuration, validationIndicatesError);

            return validationIndicatesError;
        }

        


        static bool ValidateInputConfigProjFoldPath(JsonConfiguration configuration, bool validationIndicatesError, string configFileName)
        {
            if (string.IsNullOrWhiteSpace(configuration.ProjectFolderPath))
            {
                Console.WriteLine("ProjectFolderPath is required");
                return true;
            }

            if (!Directory.Exists(configuration.ProjectFolderPath))
            {
                Console.WriteLine($"Project folder does not exist: {configuration.ProjectFolderPath};\n Check the ProjectFolderPath in {configFileName}.");
                return true;
            }
            return validationIndicatesError;
        }

        static bool ValidateInputConfigIsProgrammingProject(JsonConfiguration configuration, bool validationIndicatesError)
        {
            // IsProgrammingProject is a boolean and therefore requires
            // no additional validation after successful deserialization.
            return validationIndicatesError;
            }

        static bool ValidateInputConfigInclExt(JsonConfiguration configuration, bool validationIndicatesError)
        {
            if (configuration.IncludedExtensions == null)
            {
                Console.WriteLine("ERROR: IncludedExtensions is missing.");
                return true;
            }
            
            if (configuration.IncludedExtensions.Count == 0)
            {
                Console.WriteLine("ERROR: IncludedExtensions cannot be empty.");
                return true;
            }
            return validationIndicatesError;
        }

        static bool ValidateInputConfigIgnDir(JsonConfiguration configuration, bool validationIndicatesError)
        {
            // IgnoredDirectories is optional by design.
            // For now, leaving this field out or having it as an empty list is considered valid.
            // The user may define ignored directories if required.
            return validationIndicatesError;
        }

        static bool ValidateInputConfigIgnFil(JsonConfiguration configuration, bool validationIndicatesError)
        {
            // IgnoredFiles is optional by design.
            // For now, leaving this field out or having it as an empty list is considered valid.
            // The user may define ignored files if required.
            return validationIndicatesError;
        }

        static bool ValidateInputConfigOutput(JsonConfiguration configuration, bool validationIndicatesError)
        {
            if (configuration.Output == null)
            {
                Console.WriteLine("ERROR: Output configuration is missing.");
                return true;
            }

            // Output field OutputFileName:
            validationIndicatesError = ValidateInputConfigOutputFileName(configuration, validationIndicatesError);
            // Output field Mode:
            validationIndicatesError = ValidateInputConfigOutputMode(configuration, validationIndicatesError);
            // Output field IgnoreMode:
            validationIndicatesError = ValidateInputConfigIgnoreMode(configuration, validationIndicatesError);
            
            return validationIndicatesError;
        }

        static bool ValidateInputConfigOutputFileName(JsonConfiguration configuration, bool validationIndicatesError)
        {
            if (configuration.Output.OutputFileName == null)
            {
                Console.WriteLine("ERROR: Output field OutputFileName is missing.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(configuration.Output.OutputFileName))
            {
                Console.WriteLine("ERROR: OutputFileName cannot be empty or whitespace.");
                return true;
            }

            string extension = Path.GetExtension(configuration.Output.OutputFileName);

            if (string.IsNullOrEmpty(extension))
            {
                Console.WriteLine($"Missing file extension. Default extension will be used. Resulting in:\n\"{configuration.Output.OutputFileName}{ApplicationConfiguration.DefaultOutputFileExtension}\".");
            }
            else
            {
                if (!ApplicationConfiguration.SupportedOutputExtensions.Contains(extension))
                {
                    Console.WriteLine($"ERROR: Extension {extension} not supported.\nSupported extensions are: {string.Join(", ", ApplicationConfiguration.SupportedOutputExtensions)}");
                    return true;
                }
            }
            
            return validationIndicatesError;
        }

        static bool ValidateInputConfigOutputMode(JsonConfiguration configuration, bool validationIndicatesError)
        {
            string? thisMode = configuration.Output?.Mode;

            if (string.IsNullOrWhiteSpace(thisMode))
            {
                Console.WriteLine(
                    $"WARNING: No output mode specified. " +
                    $"Default mode will be used: {ApplicationConfiguration.DefaultOutputMode}.");
            }
            else
            {
                string[] validOutputModes = Enum.GetNames<OutputMode>();
                
                if (!validOutputModes.Contains(thisMode))
                {
                    Console.WriteLine($"ERROR: Mode {thisMode} not supported.\n");
                    Console.WriteLine($"Supported modes are: {string.Join(", ", validOutputModes)}");
                    
                    return true;
                }
            }
            
            return validationIndicatesError;
        }

        static bool ValidateInputConfigIgnoreMode(JsonConfiguration configuration, bool validationIndicatesError)
        {
            string? thisIgnoreMode = configuration.Output?.IgnoreMode;

            if (string.IsNullOrWhiteSpace(thisIgnoreMode))
            {
                Console.WriteLine(
                    $"WARNING: No output mode specified. " +
                    $"Default mode will be used: {ApplicationConfiguration.DefaultOutputIgnoreMode}.");
            }
            else
            {
                string[] validIgnoreModes = Enum.GetNames<IgnoreMode>();

                if (!validIgnoreModes.Contains(thisIgnoreMode))
                {
                    Console.WriteLine($"ERROR: Mode {thisIgnoreMode} not supported.\n");
                    Console.WriteLine($"Supported modes are: {string.Join(", ", validIgnoreModes)}");

                    return true;
                }
            }
            
            return validationIndicatesError;
        }

        static void RunWarnings(JsonConfiguration configuration)
        {
            WarnForUnrecognizedProgrammingExtensions(configuration);
        }

        static void WarnForUnrecognizedProgrammingExtensions(JsonConfiguration configuration)
        {
            if (configuration.IsProgrammingProject)
            {
                bool containsRecognizedExtension = 
                    configuration.IncludedExtensions.Any(extension => 
                        ApplicationConfiguration.RecognizedProgrammingExtensions.Contains(extension));

                if (!containsRecognizedExtension)
                {
                    Console.WriteLine("WARNING: No recognized programming language extensions found in IncludedExtensions.\n" +
                                        "The analysis will continue. Check TroubleShooting.md for details.");
                    // Since this only functions as a warning: no "return;"
                }
            }
        }

        static JsonConfiguration PreProcessJsonConfiguration(JsonConfiguration configuration)
        {
            configuration.ProcessedConfig.ProcOutputFileName = GetOutputFileName(configuration);
            configuration.ProcessedConfig.ProcMode = GetOutputMode(configuration);
            configuration.ProcessedConfig.ProcIgnoreMode = GetOutputIgnoreMode(configuration);
            configuration.ProcessedConfig.ProcIgnoredDirectories = PreProcessIgnoredDirectories(configuration);
            configuration.ProcessedConfig.ProcIgnoredFiles = PreProcessIgnoredFiles(configuration);
            
            return configuration;
        }

        static string GetOutputFileName(JsonConfiguration configuration)
        {
            string outputFileName = configuration.Output.OutputFileName;

            if (string.IsNullOrEmpty(Path.GetExtension(outputFileName)))
            {
                outputFileName += ApplicationConfiguration.DefaultOutputFileExtension;
            }

            return outputFileName;
        }

        static OutputMode GetOutputMode(JsonConfiguration configuration)
        {
            string? mode = configuration.Output?.Mode;

            if (string.IsNullOrWhiteSpace(mode))
            {
                return ApplicationConfiguration.DefaultOutputMode;
            }

            return Enum.Parse<OutputMode>(mode);
        }

        static IgnoreMode GetOutputIgnoreMode(JsonConfiguration configuration)
        {
            string? ignoreMode = configuration.Output?.IgnoreMode;

            if (string.IsNullOrWhiteSpace(ignoreMode))
            {
                return ApplicationConfiguration.DefaultOutputIgnoreMode;
            }

            return Enum.Parse<IgnoreMode>(ignoreMode);
        }

        static List<string> PreProcessIgnoredDirectories(JsonConfiguration configuration)
        {
            List<string> preprocIgnorePaths = new();

            foreach (var ignoredDirectory in configuration.IgnoredDirectories)
            {
                if (ignoredDirectory.Contains('\\') && ignoredDirectory.Contains('/'))
                {
                    Console.WriteLine($"IgnoredDirectory {ignoredDirectory} contains both / and \\.\n"
                    + "This IgnoredDirectory entry will not be used: adjust input if needed\n"
                    + "We chose not to normalise this, in case user means something by this combination.\n"
                    + "Whichever was meant by this combination, it is not supported in this Program.");
                    continue;
                }

                string preprocIgnoredDirectory = Normalisation.NormaliseIgnorePath(ignoredDirectory);

                preprocIgnorePaths.Add(preprocIgnoredDirectory);
            }

            return preprocIgnorePaths;
        }

        static List<IgnoredFile> PreProcessIgnoredFiles(JsonConfiguration configuration)
        {
            List<IgnoredFile> output = new();

            foreach (var ignoredFile in configuration.IgnoredFiles)
            {
                IgnoredFile ignoredFileInstance = new();

                string procFile = Normalisation.GeneralPathNormalise(ignoredFile);
                ignoredFileInstance.NameOnlyWithExtension = Path.GetFileName(procFile);
                string? thisPath = Path.GetDirectoryName(procFile);
                //Console.WriteLine($"thisPath: {thisPath}");
                if (!string.IsNullOrWhiteSpace(thisPath))
                {
                    ignoredFileInstance.IgnoredFileHasPath = true;
                    ignoredFileInstance.Path = Path.Combine(configuration.ProjectFolderPath, thisPath);
                    ignoredFileInstance.FullName = Path.Combine(ignoredFileInstance.Path, ignoredFileInstance.NameOnlyWithExtension);
                }
                else
                {
                    ignoredFileInstance.IgnoredFileHasPath = false;
                    ignoredFileInstance.FullName = ignoredFileInstance.NameOnlyWithExtension;
                }
                output.Add(ignoredFileInstance);
            }

            return output;
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