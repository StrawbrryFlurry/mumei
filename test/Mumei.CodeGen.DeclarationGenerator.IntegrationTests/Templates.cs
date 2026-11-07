using System.Diagnostics;

namespace Mumei.CodeGen.DeclarationGenerator.IntegrationTests;

public partial record LogCall : InterceptorTemplate {
    public required string Message { get; init; }

    public override TResult Intercept<TResult>(Call<TResult> call) {
        Console.WriteLine(Message);
        var sw = Stopwatch.StartNew();
        try {
            return call.Proceed();
        } finally {
            Console.WriteLine($"{call.Method.Name} took {sw.ElapsedMilliseconds}ms");
        }
    }
}

public partial record ClampArgument : InterceptorTemplate {
    public required int Max { get; init; }

    public override TResult Intercept<TResult>(Call<TResult> call) {
        var clamped = Math.Min(call.Arg<int>("value"), Max);
        return call.Proceed(clamped);
    }
}

public partial record Repository<TEntity> : ClassTemplate {
    public required Type<TEntity> Entity { get; init; }

    public override string Name => $"{Entity.Name}Repository";
    public override string Namespace => Entity.Namespace;

    private readonly Dictionary<int, TEntity> _items = new();

    public TEntity? Find(int id) => _items.GetValueOrDefault(id);

    public void Add(int id, TEntity entity) {
        _items[id] = entity;
        Console.WriteLine($"Added to {Name}");
    }
}

public partial record Dispatcher : ClassTemplate, IDisposable {
    public required EquatableArray<TypeRef> Requests { get; init; }

    public override Modifiers Modifiers => Modifiers.Public | Modifiers.Sealed;

    public string Describe(object request) => Dispatch(request) ?? "unknown";

    public void Dispose() { }

    [Generated]
    private static partial string? Dispatch(object request);

    [CompileTime]
    private string Header => $"Dispatching {Requests.Length} requests";

    protected override void Build(TypeBuilder self) {
        self.Implement(nameof(Dispatch), body => {
            body.Line($"// {Header}");
            body.Line("return request switch {");
            foreach (var request in Requests) {
                body.Line($"    {request} => {request.Name:literal},");
            }

            body.Line("    _ => null");
            body.Line("};");
        });
    }
}
