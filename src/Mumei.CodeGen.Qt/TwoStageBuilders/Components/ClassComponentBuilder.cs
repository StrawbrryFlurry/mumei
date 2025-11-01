using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class ClassComponentBuilder : IClassComponentBuilder {
    public ImmutableArray<IMethodComponent> Methods { get; }
}

public interface IClassComponentBuilder : IClassComponent { }

public interface IClassComponent {
    public ImmutableArray<IMethodComponent> Methods { get; }
}

public interface IMethodComponent {
    public string Name { get; }
}

internal sealed class RuntimeMethodComponent(MethodInfo method) : IMethodComponent {
    public string Name => method.Name;
}

internal sealed class RoslynMethodComponent(IMethodSymbol method) : IMethodComponent {
    public string Name => method.Name;
}

internal sealed class QtMethodComponent(string name) : IMethodComponent {
    public string Name { get; } = name;
}