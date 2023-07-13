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
            var file = AnsiConsole.Prompt(new TextPrompt<string>("Enter the path of the file to convert:")
                                             .Validate(File.Exists, "File does not exist"));
            try
            {
                await ProcessFileAsync(new FileInfo(file));
            }
            catch ( Exception e )
            {
                AnsiConsole.MarkupLine("[red]An error occurred while processing the file.[/]");
                AnsiConsole.WriteException(e);
            }

            processNextFile = AnsiConsole.Confirm("Would you like to convert another file?");
        }
    }

    private static async ValueTask ProcessFileAsync(FileInfo file)
    {
        await using var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        var tree = await ReadTreeAsync(stream);

        var recordTree = Recorder.ToRecord(tree);
        var recordText = recordTree.ToString();

        AnsiConsole.MarkupLine("[blue]Previewing record file:[/]");
        AnsiConsole.Write(new Markup(recordText, new Style(Color.Silver)));
        AnsiConsole.WriteLine();
        var write = AnsiConsole.Confirm("Are you sure you would like to overwrite the file?");

        if ( write )
        {
            AnsiConsole.MarkupLineInterpolated($"[blue]Overwriting {file.Name}...[/]");
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

    private static async ValueTask<SyntaxTree> ReadTreeAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen : true);
        var text = await reader.ReadToEndAsync();
        return CSharpSyntaxTree.ParseText(text);
    }
}
