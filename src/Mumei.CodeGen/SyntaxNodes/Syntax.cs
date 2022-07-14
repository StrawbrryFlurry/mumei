using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class Syntax
{
    public readonly string Identifier;
    public readonly Syntax? Parent;
    private List<AttributeUsage> _attributes = new();
    protected SyntaxTypeContext TypeContext;

    protected Syntax(string identifier)
    {
        Identifier = identifier;
        TypeContext = new SyntaxTypeContext();
    }

    protected Syntax(string identifier, Syntax? parent)
    {
        Identifier = identifier;
        Parent = parent;
        TypeContext = parent?.TypeContext ?? new SyntaxTypeContext();
    }

    public SyntaxVisibility Visibility { get; set; } = SyntaxVisibility.None;

    public IEnumerable<AttributeUsage> Attributes
    {
        get => _attributes;
        set => _attributes = value.ToList();
    }

    public bool HasAttributes => _attributes.Count > 0;

    protected internal string? GetAttributeSyntax(bool sameLine = false)
    {
        if (!Attributes.Any()) return null;

        var writer = new AttributeSyntaxWriter(TypeContext);
        writer.WriteAttributes(Attributes.ToArray(), sameLine);

        return writer.ToSyntax();
    }

    /// <summary>
    ///     Returns the identifier for the member.
    /// </summary>
    /// <returns></returns>
    public virtual string GetIdentifier()
    {
        return Identifier;
    }

    public Syntax SetVisibility(SyntaxVisibility visibility)
    {
        Visibility = visibility;
        return this;
    }

    public Syntax AddAttribute<TAttribute>(params object[] arguments) where TAttribute : Attribute
    {
        return AddAttribute(typeof(TAttribute), arguments);
    }

    public Syntax AddAttribute<TAttribute>(
        Dictionary<NamedAttributeParameter, object> namedArguments,
        params object[] arguments
    ) where TAttribute : Attribute
    {
        return AddAttribute(typeof(TAttribute), namedArguments, arguments);
    }

    public Syntax AddAttribute(Type attributeType, params object[] arguments)
    {
        _attributes.Add(
            new AttributeUsage(attributeType)
            {
                Arguments = arguments
            }
        );

        return this;
    }

    public Syntax AddAttribute(
        Type attributeType,
        Dictionary<NamedAttributeParameter, object> namedArguments,
        params object[] arguments
    )
    {
        _attributes.Add(
            new AttributeUsage(attributeType)
            {
                Arguments = arguments,
                NamedArguments = namedArguments
            }
        );

        return this;
    }

    public abstract void WriteAsSyntax(ITypeAwareSyntaxWriter writer);
}