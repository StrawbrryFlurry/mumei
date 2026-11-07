using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>
/// Declares a class, struct, interface or record. Member methods return builders that render as their name,
/// so they can be referenced from code: <c>cls.Method&lt;int&gt;("Count").ExpressionBody($"{items}.Count")</c>.
/// </summary>
public sealed class TypeBuilder : MemberBuilder {
    private readonly List<MemberBuilder> _members = [];
    private readonly HashSet<string> _memberNames = [];
    private readonly List<TypeRef> _baseTypes = [];
    private readonly List<(TypeRef Parameter, Code Constraint)> _typeParameters = [];
    private readonly string _namespace;
    private readonly TypeBuilder? _containingType;

    internal TypeBuilder(string name, TypeDeclarationKind kind, Modifiers modifiers, string @namespace, TypeBuilder? containingType)
        : base(name) {
        Kind = kind;
        Modifiers = modifiers;
        _namespace = @namespace;
        _containingType = containingType;
    }

    internal TypeDeclarationKind Kind { get; }
    public Modifiers Modifiers { get; set; }

    /// <summary>A reference to the type being built, e.g. for use as a parameter type.</summary>
    public TypeRef Ref {
        get {
            var typeArguments = _typeParameters.Select(x => x.Parameter).ToEquatableArray();
            var typeArgumentList = typeArguments.IsEmpty ? "" : $"<{string.Join(", ", typeArguments.Select(x => x.Name))}>";
            var qualifier = _containingType is not null
                ? _containingType.Ref.FullName + "."
                : _namespace.Length > 0
                    ? $"global::{_namespace}."
                    : "global::";

            return new TypeRef {
                FullName = qualifier + CSharpIdentifier.Escape(Name) + typeArgumentList,
                Name = Name,
                Namespace = _namespace,
                MetadataName = (_containingType is not null
                                   ? _containingType.Ref.MetadataName + "+"
                                   : _namespace.Length > 0
                                       ? _namespace + "."
                                       : "")
                               + Name + (typeArguments.IsEmpty ? "" : $"`{typeArguments.Length}"),
                TypeArguments = typeArguments,
                IsClass = Kind is TypeDeclarationKind.Class or TypeDeclarationKind.Record,
                IsStruct = Kind is TypeDeclarationKind.Struct or TypeDeclarationKind.RecordStruct,
                IsInterface = Kind is TypeDeclarationKind.Interface,
                IsRecord = Kind is TypeDeclarationKind.Record or TypeDeclarationKind.RecordStruct
            };
        }
    }

    internal bool IsEmpty => _members.Count == 0;

    internal bool HasTypeParameters => _typeParameters.Count > 0;

    public TypeBuilder Extends(TypeRef baseType) {
        _baseTypes.Insert(0, baseType);
        return this;
    }

    public TypeBuilder Extends<T>() {
        return Extends(TypeRef.Of<T>());
    }

    public TypeBuilder Implements(TypeRef @interface) {
        if (!_baseTypes.Contains(@interface)) {
            _baseTypes.Add(@interface);
        }

        return this;
    }

    public TypeBuilder Implements<T>() {
        return Implements(TypeRef.Of<T>());
    }

    /// <summary>Adds an attribute, e.g. <c>$"{typeof(ObsoleteAttribute)}(\"Use Y\")"</c>.</summary>
    public TypeBuilder Attribute(Code attribute) {
        AttributeList.Add(attribute);
        return this;
    }

    /// <summary>Declares a type parameter and returns a reference to it.</summary>
    public TypeRef TypeParameter(string name, Code constraint = default) {
        var parameter = TypeRef.TypeParameter(name);
        _typeParameters.Add((parameter, constraint));
        return parameter;
    }

    public FieldBuilder Field<T>(string name, Modifiers modifiers = Modifiers.Private, Code initializer = default) {
        return Field(TypeRef.Of<T>(), name, modifiers, initializer);
    }

    public FieldBuilder Field(TypeRef type, string name, Modifiers modifiers = Modifiers.Private, Code initializer = default) {
        return AddMember(new FieldBuilder(type, name, modifiers, initializer));
    }

    /// <summary>Declares an auto-property <c>{ get; set; }</c>; see <see cref="PropertyBuilder"/> for other shapes.</summary>
    public PropertyBuilder Property<T>(string name, Modifiers modifiers = Modifiers.Public) {
        return Property(TypeRef.Of<T>(), name, modifiers);
    }

    public PropertyBuilder Property(TypeRef type, string name, Modifiers modifiers = Modifiers.Public) {
        return AddMember(new PropertyBuilder(type, name, modifiers));
    }

    public MethodBuilder Method(string name, TypeRef returnType, Modifiers modifiers = Modifiers.Public) {
        return AddMember(new MethodBuilder(name, returnType, modifiers, isConstructor: false));
    }

    public MethodBuilder Method<TReturn>(string name, Modifiers modifiers = Modifiers.Public) {
        return Method(name, TypeRef.Of<TReturn>(), modifiers);
    }

    /// <summary>Declares a method returning <c>void</c>.</summary>
    public MethodBuilder Method(string name, Modifiers modifiers = Modifiers.Public) {
        return Method(name, TypeRef.Void, modifiers);
    }

    public MethodBuilder Constructor(Modifiers modifiers = Modifiers.Public) {
        var constructor = new MethodBuilder(Name, TypeRef.Void, modifiers, isConstructor: true);
        _members.Add(constructor);
        return constructor;
    }

    /// <summary>
    /// Declares a method with the same signature as <paramref name="method"/>,
    /// e.g. to implement an interface method.
    /// </summary>
    public MethodBuilder Implement(MethodModel method, Modifiers modifiers = Modifiers.Public) {
        var builder = Method(method.Name, method.ReturnType, modifiers);
        foreach (var typeArgument in method.TypeArguments) {
            if (typeArgument.IsTypeParameter) {
                builder.TypeParameter(typeArgument.Name);
            }
        }

        foreach (var parameter in method.Parameters) {
            builder.Parameter(parameter.Type, parameter.Name, parameter.Modifiers);
        }

        return builder;
    }

    public MethodBuilder Implement(MethodModel method, Code body) {
        return Implement(method).Body(body);
    }

    public MethodBuilder Implement(MethodModel method, Action<CodeBuilder> body) {
        return Implement(method).Body(body);
    }

    public TypeBuilder Class(string name, Modifiers modifiers = Modifiers.Public) {
        return NestedType(name, TypeDeclarationKind.Class, modifiers);
    }

    public TypeBuilder Struct(string name, Modifiers modifiers = Modifiers.Public) {
        return NestedType(name, TypeDeclarationKind.Struct, modifiers);
    }

    public TypeBuilder Interface(string name, Modifiers modifiers = Modifiers.Public) {
        return NestedType(name, TypeDeclarationKind.Interface, modifiers);
    }

    public TypeBuilder Record(string name, Modifiers modifiers = Modifiers.Public) {
        return NestedType(name, TypeDeclarationKind.Record, modifiers);
    }

    /// <summary>Adds a member from verbatim code.</summary>
    public TypeBuilder Member(Code member) {
        _members.Add(new RawMemberBuilder(member));
        return this;
    }

    /// <summary>
    /// Returns <paramref name="preferred"/>, or a numbered variant of it if a member with that name already exists.
    /// The result is deterministic for the same sequence of declarations.
    /// </summary>
    public string FreeName(string preferred) {
        if (IsFree(preferred)) {
            return preferred;
        }

        for (var i = 1;; i++) {
            var candidate = preferred + i;
            if (IsFree(candidate)) {
                return candidate;
            }
        }

        bool IsFree(string name) {
            return name != Name && !_memberNames.Contains(name);
        }
    }

    internal TypeBuilder NestedType(string name, TypeDeclarationKind kind, Modifiers modifiers) {
        foreach (var member in _members) {
            if (member is TypeBuilder existing && existing.Name == name && existing.Kind == kind) {
                existing.Modifiers |= modifiers;
                return existing;
            }
        }

        return AddMember(new TypeBuilder(name, kind, modifiers, _namespace, this));
    }

    internal TMember AddMember<TMember>(TMember member) where TMember : MemberBuilder {
        _memberNames.Add(member.Name);
        _members.Add(member);
        return member;
    }

    internal override void RenderDeclaration(IRenderTreeBuilder tree) {
        RenderAttributes(tree);
        tree.Text($"{Prefix(Modifiers)}{Kind.ToKeyword()} {CSharpIdentifier.Escape(Name)}");
        if (_typeParameters.Count > 0) {
            tree.Text($"<{string.Join(", ", _typeParameters.Select(p => p.Parameter.Name))}>");
        }

        if (_baseTypes.Count > 0) {
            tree.Text($" : {string.Join(", ", _baseTypes.Select(t => t.FullName))}");
        }

        foreach (var (parameter, constraint) in _typeParameters) {
            if (!constraint.IsEmpty) {
                tree.Text($" where {parameter.Name} : {constraint}");
            }
        }

        tree.Text(" ");
        tree.StartCodeBlock();
        for (var i = 0; i < _members.Count; i++) {
            var member = _members[i];
            var isFieldGroup = i > 0 && member is FieldBuilder && _members[i - 1] is FieldBuilder;
            if (i > 0 && !isFieldGroup) {
                tree.NewLine();
            }

            member.RenderDeclaration(tree);
        }

        tree.EndCodeBlock();
    }
}

internal enum TypeDeclarationKind {
    Class,
    Struct,
    Interface,
    Record,
    RecordStruct
}

internal static class TypeDeclarationKindExtensions {
    public static string ToKeyword(this TypeDeclarationKind kind) {
        return kind switch {
            TypeDeclarationKind.Class => "class",
            TypeDeclarationKind.Struct => "struct",
            TypeDeclarationKind.Interface => "interface",
            TypeDeclarationKind.Record => "record",
            TypeDeclarationKind.RecordStruct => "record struct",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}
