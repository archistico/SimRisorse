using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void CoreHasOnlyFrameworkAssemblyDependencies()
    {
        var references = typeof(GameSession).Assembly.GetReferencedAssemblies();
        Assert.All(references, reference => Assert.StartsWith("System.", reference.Name));
    }

    [Fact]
    public void ApplicationDependsOnlyOnCoreAndTheFramework()
    {
        var references = typeof(GameRuntime).Assembly.GetReferencedAssemblies();
        Assert.Contains(references, reference => reference.Name == "SimRisorse.Core");
        Assert.All(references, reference => Assert.True(reference.Name == "SimRisorse.Core" ||
            reference.Name!.StartsWith("System.", StringComparison.Ordinal), reference.Name));
    }
}
