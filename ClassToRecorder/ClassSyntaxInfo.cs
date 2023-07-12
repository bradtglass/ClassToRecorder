using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal record ClassSyntaxInfo(SyntaxTokenList Modifiers,
                                SyntaxToken Name,
                                BaseListSyntax? BaseList,
                                SyntaxList<AttributeListSyntax> Attributes,
                                SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses,
                                TypeParameterListSyntax? TypeParameterList,
                                SyntaxList<MemberDeclarationSyntax> RemainingMembers,
                                IReadOnlyCollection<PropertySyntaxInfo> PublicAutoProperties);
