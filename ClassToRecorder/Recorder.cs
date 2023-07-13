using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassToRecorder;

public static class Recorder
{
    public static SyntaxTree? ToRecord(SyntaxTree tree)
    {
        var root = tree.GetRoot();
        var topLevelNodes = root.ChildNodes().ToList();
        var @namespace = topLevelNodes.OfType<BaseNamespaceDeclarationSyntax>().Single();
        var usings = topLevelNodes.OfType<UsingDirectiveSyntax>().Concat(@namespace.Usings).ToList();

        var members = new List<MemberDeclarationSyntax>();
        var hasRecord = false;
        foreach ( var member in @namespace.Members )
        {
            if ( member is ClassDeclarationSyntax classSyntax )
            {
                hasRecord = true;
                var syntaxInfo = LoadClass(classSyntax);
                var recordSyntax = GenerateRecord(syntaxInfo);
                members.Add(recordSyntax);
            }
            else
            {
                members.Add(member);
            }
        }

        if ( !hasRecord )
        {
            return null;
        }

        var fileSyntaxInfo = new FileSyntaxInfo(usings,
                                                @namespace.Name,
                                                members);

        var syntax = RebuildFile(fileSyntaxInfo);
        return CSharpSyntaxTree.Create(syntax);
    }

    private static CompilationUnitSyntax RebuildFile(FileSyntaxInfo file)
    {
        var namespaceSyntax = SyntaxFactory.FileScopedNamespaceDeclaration(file.Namespace)
                                           .WithUsings(SyntaxFactory.List(file.Usings))
                                           .WithMembers(SyntaxFactory.List(file.Members));

        var syntax = SyntaxFactory.CompilationUnit()
                                  .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceSyntax.NormalizeWhitespace()));

        return syntax;
    }

    private static RecordDeclarationSyntax GenerateRecord(ClassSyntaxInfo classInfo)
    {
        var parameterList = SyntaxFactory.ParameterList();
        foreach ( var prop in classInfo.PublicAutoProperties )
        {
            var parameterSyntax = SyntaxFactory.Parameter(prop.Name).WithType(prop.Type);
            parameterList = parameterList.AddParameters(parameterSyntax);
        }

        var recordSyntax = SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                                                           classInfo.Name)
                                        .WithAttributeLists(classInfo.Attributes)
                                        .WithModifiers(classInfo.Modifiers)
                                        .WithBaseList(classInfo.BaseList)
                                        .WithTypeParameterList(classInfo.TypeParameterList)
                                        .WithConstraintClauses(classInfo.ConstraintClauses)
                                        .WithParameterList(parameterList)
                                        .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        if ( !classInfo.RemainingMembers.Any() )
        {
            recordSyntax = recordSyntax.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            recordSyntax = recordSyntax.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                                       .WithMembers(classInfo.RemainingMembers)
                                       .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }

        return recordSyntax;
    }

    private static ClassSyntaxInfo LoadClass(ClassDeclarationSyntax syntax)
    {
        var otherMembers = new SyntaxList<MemberDeclarationSyntax>();
        var autoProperties = new List<PropertySyntaxInfo>();

        foreach ( var member in syntax.Members )
        {
            if ( member is PropertyDeclarationSyntax propertyDeclarationSyntax && IsPublicAuto(propertyDeclarationSyntax) )
            {
                autoProperties.Add(new PropertySyntaxInfo(propertyDeclarationSyntax.Type,
                                                          propertyDeclarationSyntax.Identifier));
            }
            else
            {
                otherMembers = otherMembers.Add(member);
            }
        }

        return new ClassSyntaxInfo(syntax.Modifiers,
                                   syntax.Identifier,
                                   syntax.BaseList,
                                   syntax.AttributeLists,
                                   syntax.ConstraintClauses,
                                   syntax.TypeParameterList,
                                   otherMembers,
                                   autoProperties);
    }

    private static bool IsPublicAuto(PropertyDeclarationSyntax syntax) =>
        syntax.Modifiers.Count == 1 &&
        syntax.Modifiers[ 0 ].IsKind(SyntaxKind.PublicKeyword) &&
        syntax.AccessorList is not null &&
        syntax.AccessorList.Accessors.Any() &&
        syntax.AccessorList.Accessors.All(a => a.Body is null && a.ExpressionBody is null);
}
