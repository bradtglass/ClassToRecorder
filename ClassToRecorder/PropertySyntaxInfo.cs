using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal record PropertySyntaxInfo(TypeSyntax Type, SyntaxToken Name);
