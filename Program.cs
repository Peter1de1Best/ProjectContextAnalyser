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

            AnalysisOrchestrator.Run(projectFiles, configuration);



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



}