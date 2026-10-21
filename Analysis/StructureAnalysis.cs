using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
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
}