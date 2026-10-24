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

        public static List<ProjectFile> FilterIfApplicable(List<ProjectFile> projectFiles, ProcessedJsonConfiguration config, AnalysisType analysisType)
        {
            return 
                ShouldApplyIgnoreRules(config, analysisType)
                    ? PreAnalysisFiltering.FilterIgnoredProjectFiles(projectFiles, config)
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
}