using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
    static class AnalysisOrchestrator
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
                        ? PreAnalysisFiltering.FilterIgnoredProjectFiles(orderedFiles, procConfig)
                        : projectFiles;
                    

                StructureAnalysis.Run(sb, orderedProjectFilesToUse, config);
            }

            if (procConfig.RunContent)
            {
                var orderedProjectFilesToUse =
                    procConfig.IgnoreInContent
                        ? PreAnalysisFiltering.FilterIgnoredProjectFiles(orderedFiles, procConfig)
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
}