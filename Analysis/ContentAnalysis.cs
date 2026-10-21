using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
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