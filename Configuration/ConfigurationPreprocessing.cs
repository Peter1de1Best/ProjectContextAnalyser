using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
    class ConfigurationPreprocessing
    {
        
        public static JsonConfiguration PreProcess(JsonConfiguration configuration)
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
            string outputFileName = configuration.Output!.OutputFileName!; // Validation elsewhere guarantees Output and OutputFileName are not null.

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

                string preprocIgnoredDirectory = PathNormalisation.NormaliseIgnorePath(ignoredDirectory);

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

                string procFile = PathNormalisation.GeneralPathNormalise(ignoredFile);
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
    }
}