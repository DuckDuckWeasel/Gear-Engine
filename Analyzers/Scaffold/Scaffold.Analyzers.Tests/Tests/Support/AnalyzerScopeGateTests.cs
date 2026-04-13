using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Scaffold.Analyzers.Tests;

public sealed class AnalyzerScopeGateTests
{
    [Fact]
    public async Task ExpressionBody_NoDiagnostic_WhenIgnoredAssemblyNameContainsMatches()
    {
        const string source = @"
namespace Demo
{
    public class Sample
    {
        public int Count() => 1;
    }
}";

        var options = new Dictionary<string, string>
        {
            [AnalyzerScopeGate.IgnoredAssemblyNameContainsKey] = "Tests",
        };

        var diagnostics = await AnalyzerTestHarness.GetDiagnosticsByIdAsync(
            source,
            @"C:\Repo\Assets\Scripts\Core\Sample.cs",
            new ExpressionBodiedMethodAnalyzer(),
            ExpressionBodiedMethodAnalyzer.DiagnosticId,
            analyzerOptions: options,
            compilationAssemblyName: "Game.GearEngine.Tests");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ExpressionBody_NoDiagnostic_WhenIgnoredPathMatches()
    {
        const string source = @"
namespace Demo
{
    public class Sample
    {
        public int Count() => 1;
    }
}";

        var options = new Dictionary<string, string>
        {
            [AnalyzerScopeGate.IgnoredPathsKey] = "Assets/Samples",
        };

        var diagnostics = await AnalyzerTestHarness.GetDiagnosticsByIdAsync(
            source,
            @"C:\Repo\Assets\Samples\Splines\Runtime\Sample.cs",
            new ExpressionBodiedMethodAnalyzer(),
            ExpressionBodiedMethodAnalyzer.DiagnosticId,
            analyzerOptions: options,
            compilationAssemblyName: "Unity.Splines.Examples");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ExpressionBody_Diagnostic_WhenNoIgnoreMatch()
    {
        const string source = @"
namespace Demo
{
    public class Sample
    {
        public int Count() => 1;
    }
}";

        var options = new Dictionary<string, string>
        {
            [AnalyzerScopeGate.IgnoredAssemblyNameContainsKey] = "Tests",
            [AnalyzerScopeGate.IgnoredPathsKey] = "Assets/Samples",
        };

        var diagnostics = await AnalyzerTestHarness.GetDiagnosticsByIdAsync(
            source,
            @"C:\Repo\Assets\Scripts\Core\Sample.cs",
            new ExpressionBodiedMethodAnalyzer(),
            ExpressionBodiedMethodAnalyzer.DiagnosticId,
            analyzerOptions: options,
            compilationAssemblyName: "Game.Core.Runtime");

        Assert.Single(diagnostics);
    }

    [Fact]
    public async Task ExpressionBody_NoDiagnostic_WhenIgnoreInfrastructureAssembliesTrue_AndEditorAssembly()
    {
        const string source = @"
namespace Demo
{
    public class Sample
    {
        public int Count() => 1;
    }
}";

        var options = new Dictionary<string, string>
        {
            [AnalyzerScopeGate.IgnoreInfrastructureAssembliesKey] = "true",
        };

        var diagnostics = await AnalyzerTestHarness.GetDiagnosticsByIdAsync(
            source,
            @"C:\Repo\Assets\Scripts\Core\Editor\Sample.cs",
            new ExpressionBodiedMethodAnalyzer(),
            ExpressionBodiedMethodAnalyzer.DiagnosticId,
            analyzerOptions: options,
            compilationAssemblyName: "Game.Core.Editor");

        Assert.Empty(diagnostics);
    }
}
