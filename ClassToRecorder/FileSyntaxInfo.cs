using Microsoft.CodeAnalysis.CSharp.Syntax;

internal record FileSyntaxInfo(IReadOnlyCollection<UsingDirectiveSyntax> Usings,
                               NameSyntax Namespace,
                               IReadOnlyCollection<ClassSyntaxInfo> Classes);
