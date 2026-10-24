using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
    class ConfigurationValidation
    {
        public static bool Validate(RetrievedInputInformation inputInfo)
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

            ValidateConfigurationWarnings(configuration); // Validation above guarantees configuration is not null here.

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
            if (configuration.Output!.OutputFileName == null) // Validation elsewhere guarantees Output is not null. 
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

        static void ValidateConfigurationWarnings(JsonConfiguration configuration)
        {
            WarnForUnrecognizedProgrammingExtensions(configuration);
        }

        static void WarnForUnrecognizedProgrammingExtensions(JsonConfiguration configuration)
        {
            if (configuration.IsProgrammingProject)
            {
                // Validation elsewhere guarantees IncludedExtensions is not null. 
                bool containsRecognizedExtension = 
                    configuration.IncludedExtensions!.Any(extension => 
                        ApplicationConfiguration.RecognizedProgrammingExtensions.Contains(extension));

                if (!containsRecognizedExtension)
                {
                    Console.WriteLine("WARNING: No recognized programming language extensions found in IncludedExtensions.\n" +
                                        "The analysis will continue. Check TroubleShooting.md for details.");
                    // Since this only functions as a warning: no "return;"
                }
            }
        }
    }
        

}