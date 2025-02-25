using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using XunitToMSTestConverter.Helpers;

namespace XunitToMSTestConverter;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class XunitAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor AttributeRule = new(
       "XunitToMSTest001",
       title: "Replace xUnit attributes with MSTest equivalent",
       messageFormat: "Use MSTest attributes",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [AttributeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(context =>
        {
            WellKnownTypeProvider wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
            context.RegisterOperationAction(context => AnalyzeAttribute(context, wellKnownTypeProvider), OperationKind.Attribute);

        });
    }

    private static void AnalyzeAttribute(OperationAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider)
    {
        var attribute = (IAttributeOperation)context.Operation;
        if (attribute.Operation.Type is not { } attributeType)
        {
            return;
        }

        ReportIfAttributeFound(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitFactAttribute));
        ReportIfAttributeFound(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitTheoryAttribute));

        ReportIfAttributeFound(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitInlineDataAttribute));
        ReportIfAttributeFound(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitMemberDataAttribute));

        // Local functions
        void ReportIfAttributeFound(INamedTypeSymbol? type)
        {
            if (type is not null && attributeType.Inherits(type))
            {
                context.ReportDiagnostic(attribute.CreateDiagnostic(AttributeRule));
            }
        }
    }
}
