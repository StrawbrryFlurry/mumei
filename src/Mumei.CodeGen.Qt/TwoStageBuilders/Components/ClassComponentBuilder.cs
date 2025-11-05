using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class SyntheticClassBuilder<TClassDef> : ISyntheticClassBuilder<TClassDef> {
    public ImmutableArray<ISyntheticMethod> Methods { get; }

    public TClassDef New(object[] args) {
        throw new NotImplementedException();
    }
}

public interface ISyntheticClassBuilder<T> {
    public T New(object[] args);
}

public interface ISyntheticClass : ISyntheticType {
    public ImmutableArray<ISyntheticMethod> Methods { get; }
}

public interface ISyntheticMethodBuilder { }

public interface ISyntheticMethod {
    public string Name { get; }
}

internal sealed class RuntimeSyntheticMethod(MethodInfo method) : ISyntheticMethod {
    public string Name => method.Name;
}

internal sealed class RoslynSyntheticMethod(IMethodSymbol method) : ISyntheticMethod {
    public string Name => method.Name;
}

internal sealed class QtSyntheticMethod(string name) : ISyntheticMethod {
    public string Name { get; } = name;
}