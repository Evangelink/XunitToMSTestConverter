using Microsoft.CodeAnalysis;

namespace XunitToMSTestConverter.Helpers;

internal static class WellKnownTypeNames
{
    public const string MSTestDataRowAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute";
    public const string MSTestDynamicDataAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.DynamicDataAttribute";
    public const string MSTestTestClassAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
    public const string MSTestTestMethodAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";

    public const string SystemThreadingTasksTask1 = "System.Threading.Tasks.Task`1";

    public const string XunitFactAttribute = "Xunit.FactAttribute";
    public const string XunitInlineDataAttribute = "Xunit.InlineDataAttribute";
    public const string XunitMemberDataAttribute = "Xunit.MemberDataAttribute";
    public const string XunitTheoryAttribute = "Xunit.TheoryAttribute";
}
