# IMPORTANT NOTE
This project, and the readme file are a WIP, **though the code in its current iteration is fully functional and ready to test.** 
On a branch, I am currently working on reformatting so that not everything remains in program.cs.
When I have done that, and tested everything and verified it still works fine, I will update the readme so that it becomes (more) complete.

If you want to experiment with the code in the meantime, you can run it using "dotnet run input.json" and see what happens. 
If you wonder what field contents are allowed in input.json, you will for now have to check whether it is explained below, 
or check the code to see what is currently being supported.

# ProjectContextAnalyser
ProjectContextAnalyser is a tool that analyzes a project directory and generates either a directory/file structure overview or a complete project dump including file contents. The analysis can be customized through an input JSON file that allows specific files and directories to be excluded from the output.

# 1. Filling in the input .json file

Here I give extra information for fields that require additional explanation.
If a field is not described here, the assumption is that the corresponding
`input.json` field is sufficiently clear for now.


## IgnoredDirectories

Specify directories to ignore using their relative path from the ProjectFolderPath.

For example, if your ProjectFolderPath is:

C:\Users\xxx\OneDrive\Documenten\Projects\TestProject\

and the project contains these directories:

C:\Users\xxx\OneDrive\Documenten\Projects\TestProject\obj\Debug\
C:\Users\xxx\OneDrive\Documenten\Projects\TestProject\Debug\

and you only want to ignore the first directory, specify:

"obj\\Debug\\"

in IgnoredDirectories.

This will ignore:
TestProject\obj\Debug\

but not:
TestProject\Debug\

## IgnoredFiles

Each entry in IgnoredFiles is processed individually, so the considerations below apply to each 
entry separately.

If you know that the filename is unique within the project, you can specify only the filename.
Note that the file extension SHOULD be included. 

For example, if PreprocClass.cs occurs only once in the project, you can specify:

"PreprocClass.cs"

The program will then ignore the file with that filename, regardless of its directory.

If multiple files with the same filename exist, specify the relative path from 
`ProjectFolderPath` to identify the specific file you want to ignore.

For example, if your ProjectFolderPath is:

C:\Users\xxx\OneDrive\Documenten\Projects\TestProject\

and the project contains these files:

C:\Users\xxx\OneDrive\Documenten\Projects\TestProject\Models\Management\User.cs
C:\Users\xxx\OneDrive\Documenten\Projects\TestProject\Models\Accounts\User.cs

and you only want to ignore the first file, specify:

"Models\\Management\\User.cs"

in IgnoredFiles.

This will ignore:
TestProject\Models\Management\User.cs

but not:
TestProject\Models\Accounts\User.cs


# 2. Notes on how the tool works:
- Using the variable *BinaryExclusionForContentAnalysis* in ***Application.Configuration.cs***, we exclude a number of known binary file formats to keep the generated output readable. 

This list is not exhaustive. If you encounter unreadable output, exclude the extension from IncludedExtensions or add the extension to the known binary formats list.















Extra info:
ProjectFile
│
├── FullName                  absolute
├── Path                      relative directory
├── NameOnlyWithExtension
├── NameOnlyWithoutExtension
└── Extension
