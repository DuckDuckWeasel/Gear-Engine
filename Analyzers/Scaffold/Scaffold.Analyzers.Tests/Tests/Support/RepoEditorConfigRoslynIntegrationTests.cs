using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Scaffold.Analyzers.Tests;

/// <summary>
/// Verifies how Roslyn resolves <c>.editorconfig</c> for repo-style paths (Unity layout).
/// </summary>
public sealed class RepoEditorConfigRoslynIntegrationTests
{
    [Fact]
    public void IgnoredAssemblyNameContains_IsPresent_InAnalyzerOptions_ForNestedCsPath()
    {
        var repoRoot = FindRepositoryRoot();
        var editorConfigPath = Path.Combine(repoRoot, ".editorconfig");
        Assert.True(File.Exists(editorConfigPath), $"Missing {editorConfigPath}");

        var configText = File.ReadAllText(editorConfigPath);
        // Use global:: to avoid confusion with Scaffold.Analyzers.AnalyzerConfig (config helper type).
        var analyzerConfig = global::Microsoft.CodeAnalysis.AnalyzerConfig.Parse(configText, editorConfigPath);
        var set = global::Microsoft.CodeAnalysis.AnalyzerConfigSet.Create(new[] { analyzerConfig }, out var diagnostics);
        Assert.Empty(diagnostics);

        // Path need not exist on disk; Roslyn matches globs against the string only.
        var nestedEditorCs = Path.Combine(
            repoRoot,
            "Assets",
            "Scripts",
            "Game",
            "GearEngine",
            "Editor",
            "SetupBasicConfigsTool.cs");

        var result = set.GetOptionsForSourcePath(nestedEditorCs);
        Assert.Empty(result.Diagnostics);

        Assert.True(
            result.AnalyzerOptions.TryGetValue(AnalyzerScopeGate.IgnoredAssemblyNameContainsKey, out var raw),
            $"Expected '{AnalyzerScopeGate.IgnoredAssemblyNameContainsKey}' in AnalyzerOptions for path under [*.cs]. " +
            $"Keys present: {string.Join(", ", result.AnalyzerOptions.Keys)}");

        // Roslyn's .editorconfig parser treats ';' as starting an inline comment, so lists must use commas in .editorconfig.
        Assert.Equal("tests,samples,examples,editor", raw, ignoreCase: true);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var editorConfig = Path.Combine(directory.FullName, ".editorconfig");
            var analyzersDir = Path.Combine(directory.FullName, "Analyzers");
            if (File.Exists(editorConfig) && Directory.Exists(analyzersDir))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repository root (folder with .editorconfig and Analyzers/). BaseDirectory=" +
            AppContext.BaseDirectory);
    }
}
