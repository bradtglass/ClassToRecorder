using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spectre.Console;

namespace ClassToRecorder;

internal static class Program
{
    public static async Task Main()
    {
        var processNextFile = true;
        while ( processNextFile )
        {
            var path = AnsiConsole.Prompt(new TextPrompt<string>("Enter the path of the file or folder to convert:")
                                             .Validate(p => File.Exists(p) || Directory.Exists(p), "File does not exist"));

            if ( File.Exists(path) )
            {
                await ProcessFileSafeAsync(new FileInfo(path));
            }
            else
            {
                await ProcessFolderAsync(new DirectoryInfo(path));
            }

            processNextFile = AnsiConsole.Confirm("Would you like to convert another file?");
        }
    }

    private static async ValueTask ProcessFolderAsync(DirectoryInfo directory)
    {
        var searchOption = AnsiConsole.Prompt(new SelectionPrompt<SearchOption>()
                                             .AddChoices(SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)
                                             .Title("Select search mode:"));

        var files = directory.EnumerateFiles("*.cs", searchOption);

        var count = 0;
        var errorCount = 0;
        foreach ( var file in files )
        {
            count++;
            var success = await ProcessFileSafeAsync(file);
            if ( !success )
            {
                errorCount++;
            }

            if ( errorCount == 5 )
            {
                var cancel = AnsiConsole.Confirm("[red]It looks like you are experiencing lots of errors, would you like to cancel?[/]", false);
                if ( cancel )
                {
                    return;
                }
            }
        }

        if ( count == 0 )
        {
            AnsiConsole.MarkupLine("[red]No files found to convert.[/]");
        }
    }

    private static async ValueTask<bool> ProcessFileSafeAsync(FileInfo file)
    {
        try
        {
            await ProcessFileAsync(file);
            return true;
        }
        catch ( Exception e )
        {
            AnsiConsole.MarkupLineInterpolated($"[red]An error occurred while processing the file ({file.Name}).[/]");
            AnsiConsole.WriteException(e);

            return true;
        }
    }

    private static async ValueTask ProcessFileAsync(FileInfo file)
    {
        var fileName = WriteFileHeader(file);

        await using var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        var tree = await ReadTreeAsync(stream);

        var recordTree = Recorder.ToRecord(tree);

        if ( recordTree is null )
        {
            AnsiConsole.MarkupLine("[blue]No classes found to convert.[/]");
            return;
        }

        var recordText = recordTree.ToString();

        AnsiConsole.MarkupLine("[blue]Previewing record file:[/]");
        AnsiConsole.Write(new Markup(recordText, new Style(Color.Silver)));
        AnsiConsole.WriteLine();
        var write = AnsiConsole.Confirm($"Are you sure you would like to overwrite {fileName}?");

        if ( write )
        {
            AnsiConsole.MarkupLineInterpolated($"[blue]Overwriting {fileName}...[/]");
            stream.Position = 0;
            stream.SetLength(0);

            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(recordText);
            await writer.FlushAsync();
            AnsiConsole.MarkupLineInterpolated($"[blue]Finished writing to file.[/]");
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[blue]Cancelling and closing file.[/]");
        }
    }

    private static string WriteFileHeader(FileInfo file)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        var fileName = file.Name;
        var headerBorder = new string(Enumerable.Repeat('-', fileName.Length + 6).ToArray()) + Environment.NewLine;
        var style = new Style(Color.Magenta1);
        AnsiConsole.Write(new Markup(headerBorder, style));
        AnsiConsole.Write(new Markup($"-- {fileName} --" + Environment.NewLine, style));
        AnsiConsole.Write(new Markup(headerBorder, style));
        return fileName;
    }

    private static async ValueTask<SyntaxTree> ReadTreeAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen : true);
        var text = await reader.ReadToEndAsync();
        return CSharpSyntaxTree.ParseText(text);
    }
}
