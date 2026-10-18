# No recognised programming language extensions

## Question
*I get the warning *
```
WARNING: No recognized programming language extensions found in IncludedExtensions.
The analysis will continue. Check TroubleShooting.md for details.
```
*Why?*

## Explanation
This warning does not mean that the application cannot be used for
non-programming projects, nor does it mean that the application cannot
process programming languages that it does not recognize.

The warning is only intended as a check when IsProgrammingProject is set
to true.

Currently, ProjectContextAnalyser recognizes the following commonly used
programming languages for this check:
- Javascript - `.js`
- Python - `.py`
- Typescript - `.ts`
- Java - `.java`
- C# - `.cs`
- C++ - `.cpp`, `.cc`, `.cxx`
- C - `.c`
- PHP - `.php`
- Go - `.go`
- Rust - `.rs`
- Visual basic / VB6 - `.vb`, `.bas`, `.frm`, `.cls`

The list is only used to determine whether a warning should be shown.
It does not restrict which file extensions the application can analyse.

If convenient, you can add programming language extensions that you use
to `RecognizedProgrammingExtensions` in `ApplicationConfiguration.cs`.

You can even limit this list to the programming languages you normally use.
This can make the warning more relevant to your own projects.


### If you are not analysing a programming project
If `IsProgrammingProject` is set to `false`, this warning should not be
shown.

Check the value in your input `xxx.json` file if the warning appears unexpectedly.


### If you are analysing a programming project
Check whether the required programming language extensions are included in
`IncludedExtensions` in your input `xxx.json` file.

You can also check the generated output for an exclusion summary:
```
# Project analysis

Files discovered: xx
Files included: yy
Files excluded: zz

## Exclusion summary

- By extension: **aa**
- By directory: bb
- By filename: cc
```

`By extension` shows how many discovered files were excluded because their
extension was not included in `IncludedExtensions`.

