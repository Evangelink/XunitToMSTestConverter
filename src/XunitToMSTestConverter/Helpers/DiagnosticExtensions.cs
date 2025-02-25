using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace XunitToMSTestConverter.Helpers;

internal static class DiagnosticExtensions
{
    public static Diagnostic CreateDiagnostic(
        this SyntaxNode node,
        DiagnosticDescriptor rule,
        params object[] args)
        => node.CreateDiagnostic(rule, [], properties: null, args);

    public static Diagnostic CreateDiagnostic(
        this SyntaxNode node,
        DiagnosticDescriptor rule,
        ImmutableDictionary<string, string?>? properties,
        params object[] args)
        => node.CreateDiagnostic(rule, [], properties, args);

    public static Diagnostic CreateDiagnostic(
        this SyntaxNode node,
        DiagnosticDescriptor rule,
        ImmutableArray<Location> additionalLocations,
        ImmutableDictionary<string, string?>? properties,
        params object[] args)
        => node
            .GetLocation()
            .CreateDiagnostic(rule, additionalLocations, properties, args);

    public static Diagnostic CreateDiagnostic(
        this IOperation operation,
        DiagnosticDescriptor rule,
        params object[] args)
        => operation.Syntax.CreateDiagnostic(rule, [], properties: null, args);

    public static Diagnostic CreateDiagnostic(
        this IOperation operation,
        DiagnosticDescriptor rule,
        ImmutableDictionary<string, string?>? properties,
        params object[] args) 
        => operation.Syntax.CreateDiagnostic(rule, [], properties, args);

    public static Diagnostic CreateDiagnostic(
        this IOperation operation,
        DiagnosticDescriptor rule,
        ImmutableArray<Location> additionalLocations,
        ImmutableDictionary<string, string?>? properties,
        params object[] args) 
        => operation.Syntax.CreateDiagnostic(rule, additionalLocations, properties, args);

    public static Diagnostic CreateDiagnostic(
        this Location location,
        DiagnosticDescriptor rule,
        ImmutableArray<Location> additionalLocations,
        ImmutableDictionary<string, string?>? properties,
        params object[] args)
    {
        if (!location.IsInSource)
        {
            location = Location.None;
        }

        return Diagnostic.Create(
            descriptor: rule,
            location: location,
            additionalLocations: additionalLocations,
            properties: properties,
            messageArgs: args);
    }
}
