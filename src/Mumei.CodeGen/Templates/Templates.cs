using System.ComponentModel;

namespace Mumei.CodeGen;

/// <summary>
/// Base of all templates. A template is a <c>partial record</c> whose members are emitted as generated code.
/// <list type="bullet">
///   <item><c>required</c> properties are inputs. They are evaluated while generating and baked into the output.</item>
///   <item>Overridden hooks (<c>Name</c>, <c>Build</c>, …) and <see cref="CompileTimeAttribute">[CompileTime]</see>
///   members run while generating. They are not emitted.</item>
///   <item>Every other member is emitted as written.</item>
/// </list>
/// Two templates are equal when their inputs are equal, so a template can be used as a cache key in the incremental pipeline.
/// </summary>
public abstract record Template {
    private protected static InvalidOperationException NotProcessed(Type template) {
        return new InvalidOperationException(
            $"The template '{template.Name}' was not processed by the Mumei template generator. "
            + "Make sure it is declared as a 'partial record' and that Mumei.CodeGen.DeclarationGenerator is referenced as an analyzer."
        );
    }
}

/// <summary>
/// A template for a class. Inputs referenced in emitted members are inlined. Type parameters of the template are
/// bound by <see cref="Type{T}"/> inputs:
/// <code>
/// public partial record Repository&lt;TEntity&gt; : ClassTemplate {
///     public required Type&lt;TEntity&gt; Entity { get; init; }
///     public override string Name => $"{Entity.Name}Repository";
///
///     private readonly Dictionary&lt;int, TEntity&gt; _items = new();
///     public TEntity? Find(int id) => _items.GetValueOrDefault(id);
/// }
/// </code>
/// A generic template is instantiated through a generated non-generic sibling record: <c>new Repository { Entity = type }</c>.
/// </summary>
public abstract record ClassTemplate : Template {
    /// <summary>The name of the generated class. Defaults to the template name.</summary>
    public virtual string Name => GetType().Name;

    /// <summary>The namespace of the generated class when it is added through <c>output.Add(template)</c>.</summary>
    public virtual string Namespace => "Mumei.Generated";

    public virtual Modifiers Modifiers => Modifiers.Public;

    /// <summary>
    /// Adds members that can't be written statically, e.g. bodies of
    /// <see cref="GeneratedAttribute">[Generated]</see> methods through <see cref="TypeBuilder.Implement(string, Action{CodeBuilder})"/>.
    /// Runs after the template members have been added.
    /// </summary>
    protected virtual void Build(TypeBuilder self) { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual void EmitMembers(TypeBuilder self) {
        throw NotProcessed(GetType());
    }

    internal TypeBuilder Render(NamespaceBuilder ns) {
        var self = ns.Class(Name, Modifiers);
        EmitMembers(self);
        Build(self);
        return self;
    }
}

/// <summary>
/// A template for an interceptor. The body of <see cref="Intercept{TResult}"/> becomes the body of the interceptor,
/// and <c>call</c> describes the intercepted call:
/// <code>
/// public partial record LogCall : InterceptorTemplate {
///     public required string Message { get; init; }
///
///     public override TResult Intercept&lt;TResult&gt;(Call&lt;TResult&gt; call) {
///         Console.WriteLine(Message);
///         return call.Proceed();
///     }
/// }
/// </code>
/// </summary>
public abstract record InterceptorTemplate : Template {
    public abstract TResult Intercept<TResult>(Call<TResult> call);

    /// <summary>Customizes the interceptor (e.g. its class name) after the template body has been added.</summary>
    protected virtual void Build(InterceptorBuilder interceptor) { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual void EmitInterceptor(InterceptorBuilder interceptor, Call<object?> call) {
        throw NotProcessed(GetType());
    }

    internal void Render(InterceptorBuilder interceptor) {
        EmitInterceptor(interceptor, new Call<object?>(interceptor));
        Build(interceptor);
    }
}

/// <summary>Marks a template member that runs while generating. The member is not emitted.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class CompileTimeAttribute : Attribute;

/// <summary>
/// Marks a <c>partial</c> template method whose body is supplied in <c>Build</c> through
/// <see cref="TypeBuilder.Implement(string, Action{CodeBuilder})"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedAttribute : Attribute;
