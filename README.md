# Class To Record(er)

A simple C# console application to convert *.cs files container classes to their equivalent records.

## Usage

Ensure you have the [dotnet SDK installed](https://dotnet.microsoft.com/download).

1. Clone the repository
2. Open a terminal in the repository folder
3. Run the following command:

```shell
dotnet run --project ./ClassToRecorder
```

4. Follow the CLI instructions: 
   1. Enter the path to either 
      - An individual *.cs file or...
      - A folder containing the *.cs files you wish to convert
   2. If running over a folder, choose if you want to scan nested subfolders
   3. Review the preview of the changes for each file and choose if you want to apply them

## Issues
If you have any issues, suggestions for enhancement or anything else that might be valuable please take a minute to [open an issue](https://github.com/bradtglass/ClassToRecorder/issues/new) and consider contributing to the project via [a PR](https://github.com/bradtglass/ClassToRecorder/pulls).