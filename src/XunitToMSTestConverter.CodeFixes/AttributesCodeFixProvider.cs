using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;

using XunitToMSTestConverter.Helpers;

namespace XunitToMSTestConverter;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttributesCodeFixProvider)), Shared]
public class AttributesCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [XunitAnalyzer.AttributeRule.Id];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        if (root?.FindNode(diagnosticSpan, getInnermostNodeForTie: false) is not { } attribute)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use MSTest attribute",
                createChangedDocument: _ => FixAttributeAsync(context.Document, root, attribute, context.CancellationToken),
                equivalenceKey: nameof(AttributesCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> FixAttributeAsync(Document document, SyntaxNode root, SyntaxNode attribute, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var compilation = editor.SemanticModel.Compilation;
        var generator = editor.Generator;
        var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilation);

        if (editor.SemanticModel.GetOperation(attribute, cancellationToken) is not IAttributeOperation attributeOperation
            || wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MSTestTestClassAttribute) is not { } testClassAttribute)
        {
            return editor.GetChangedDocument();
        }

        //if (attribute.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault() is { } classDeclarationSyntax)
        //{
        //    editor.ReplaceNode(
        //        classDeclarationSyntax,
        //        generator.AddAttributes(
        //            classDeclarationSyntax,
        //            generator.Attribute(
        //                generator.TypeExpression(testClassAttribute).WithAddImportsAnnotation())));
        //}

        if (SymbolEqualityComparer.Default.Equals(attributeOperation.Operation.Type, wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitInlineDataAttribute)))
        {
            ReplaceAttribute(attribute, WellKnownTypeNames.MSTestDataRowAttribute, editor, wellKnownTypeProvider);
        }
        else if (SymbolEqualityComparer.Default.Equals(attributeOperation.Operation.Type, wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitMemberDataAttribute)))
        {
            ReplaceAttribute(attribute, WellKnownTypeNames.MSTestDynamicDataAttribute, editor, wellKnownTypeProvider);
        }
        else if (SymbolEqualityComparer.Default.Equals(attributeOperation.Operation.Type, wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitTheoryAttribute))
            || SymbolEqualityComparer.Default.Equals(attributeOperation.Operation.Type, wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitFactAttribute)))
        {
            ReplaceAttribute(attribute, WellKnownTypeNames.MSTestTestMethodAttribute, editor, wellKnownTypeProvider);
        }

        return editor.GetChangedDocument();
    }

    private static void ReplaceAttribute(SyntaxNode attribute, string newAttributeFullName, DocumentEditor editor, WellKnownTypeProvider wellKnownTypeProvider)
    {
        editor.ReplaceNode(
            attribute,
            editor.Generator.Attribute(
                editor.Generator.TypeExpression(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(newAttributeFullName)!)
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation))
                .WithAdditionalAnnotations(Simplifier.Annotation));
    }
}
