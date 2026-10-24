using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
    static class PreAnalysisFiltering
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
    }
}