using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = XunitToMSTestConverter.Test.CSharpCodeFixVerifier<
    XunitToMSTestConverter.XunitAnalyzer,
    XunitToMSTestConverter.AttributesCodeFixProvider>;

namespace XunitToMSTestConverter.Test;

[TestClass]
public class XunitToMSTestConverterUnitTest
{
    [TestMethod]
    public async Task FactMethod_Diagnostic()
    {
        var code = """
            using Xunit;
            namespace ConsoleApplication1
            {
                public class TestClass
                {   
                    [{|#0:Fact|}]
                    public void Test()
                    {
                    }
                }
            }            
            """;

        var fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;
            namespace ConsoleApplication1
            {
                public class TestClass
                {   
                    [TestMethod]
                    public void Test()
                    {
                    }
                }
            }            
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code, 
            VerifyCS.Diagnostic(XunitAnalyzer.AttributeRule).WithLocation(0), 
            fixedCode);
    }
}
