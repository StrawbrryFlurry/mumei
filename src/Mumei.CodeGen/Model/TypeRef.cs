using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>
/// An equatable reference to a type. Renders as its globally qualified name, so it can be used
/// directly in generated code: <c>$"new {type}()"</c>.
/// </summary>
public sealed record TypeRef : IRenderFragment {
    /// <summary>The globally qualified C# name, e.g. <c>global::System.Collections.Generic.List&lt;int&gt;</c>.</summary>
    public required string FullName { get; init; }

    /// <summary>The simple name without type arguments, e.g. <c>List</c>.</summary>
    public required string Name { get; init; }

    /// <summary>The containing namespace, or an empty string for the global namespace.</summary>
    public string Namespace { get; init; } = "";

    /// <summary>The metadata name of the type definition, e.g. <c>System.Collections.Generic.List`1</c>.</summary>
    public string MetadataName { get; init; } = "";

    public EquatableArray<TypeRef> TypeArguments { get; init; } = EquatableArray<TypeRef>.Empty;

    /// <summary>The element type of an array.</summary>
    public TypeRef? ElementType { get; init; }

    public bool IsClass { get; init; }
    public bool IsStruct { get; init; }
    public bool IsInterface { get; init; }
    public bool IsEnum { get; init; }
    public bool IsDelegate { get; init; }
    public bool IsRecord { get; init; }
    public bool IsArray { get; init; }
    public bool IsTypeParameter { get; init; }

    public bool IsValueType => IsStruct || IsEnum;
    public bool IsVoid => FullName == "void";
    public bool IsGeneric => !TypeArguments.IsEmpty;

    /// <summary>Whether this type is or contains an unbound type parameter, e.g. <c>List&lt;T&gt;</c>.</summary>
    public bool ContainsTypeParameters {
        get {
            if (IsTypeParameter || ElementType?.ContainsTypeParameters == true) {
                return true;
            }

            foreach (var argument in TypeArguments) {
                if (argument.ContainsTypeParameters) {
                    return true;
                }
            }

            return false;
        }
    }

    public static TypeRef Void { get; } = Of(typeof(void));

    /// <summary>Creates a reference to a type parameter, e.g. <c>T</c>.</summary>
    public static TypeRef TypeParameter(string name) {
        return new TypeRef { FullName = name, Name = name, MetadataName = name, IsTypeParameter = true };
    }

    public static TypeRef Of<T>() {
        return Of(typeof(T));
    }

    public static TypeRef Of(Type type) {
        var name = type.IsGenericParameter || type.IsArray ? type.Name : StripArity(type.Name);
        var typeArguments = type.IsGenericType && !type.IsArray
            ? type.GetGenericArguments().Select(Of).ToEquatableArray()
            : EquatableArray<TypeRef>.Empty;

        if (Nullable.GetUnderlyingType(type) is not null) {
            name = "Nullable";
        }

        return new TypeRef {
            FullName = RuntimeTypeSerializer.GetTypeFullName(type),
            Name = name,
            Namespace = type.IsGenericParameter ? "" : type.Namespace ?? "",
            MetadataName = type.IsGenericParameter ? type.Name : RuntimeTypeSerializer.GetMetadataName(type),
            TypeArguments = typeArguments,
            ElementType = type.IsArray ? Of(type.GetElementType()!) : null,
            IsClass = type.IsClass && !type.IsArray && !typeof(Delegate).IsAssignableFrom(type) && !type.IsGenericParameter,
            IsStruct = type.IsValueType && !type.IsEnum && !type.IsGenericParameter,
            IsInterface = type.IsInterface,
            IsEnum = type.IsEnum,
            IsDelegate = typeof(Delegate).IsAssignableFrom(type) && type != typeof(Delegate) && type != typeof(MulticastDelegate),
            IsRecord = IsRecordType(type),
            IsArray = type.IsArray,
            IsTypeParameter = type.IsGenericParameter
        };
    }

    /// <summary>
    /// Creates a reference to a type by name, e.g. <c>TypeRef.Named("MyApp.Models.User")</c>.
    /// Prefer <see cref="Of{T}"/> or snapshots, which also carry the kind of the type.
    /// </summary>
    public static TypeRef Named(string fullName) {
        var name = fullName.StartsWith("global::", StringComparison.Ordinal) ? fullName["global::".Length..] : fullName;
        var genericStart = name.IndexOf('<');
        var nonGenericPart = genericStart >= 0 ? name[..genericStart] : name;
        var lastDot = nonGenericPart.LastIndexOf('.');

        return new TypeRef {
            FullName = lastDot >= 0 ? $"global::{name}" : name,
            Name = lastDot >= 0 ? nonGenericPart[(lastDot + 1)..] : nonGenericPart,
            Namespace = lastDot >= 0 ? nonGenericPart[..lastDot] : "",
            MetadataName = nonGenericPart
        };
    }

    /// <summary>Whether this is the type with the given metadata name, e.g. <c>"System.Collections.Generic.List`1"</c>.</summary>
    public bool Is(string metadataName) {
        return MetadataName == metadataName;
    }

    public bool Is<T>() {
        return FullName == RuntimeTypeSerializer.GetTypeFullName(typeof(T));
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(FullName);
    }

    public override string ToString() {
        return FullName;
    }

    private static string StripArity(string name) {
        var tick = name.IndexOf('`');
        return tick >= 0 ? name[..tick] : name;
    }

    private static bool IsRecordType(Type type) {
        if (type.IsGenericParameter || type.IsInterface) {
            return false;
        }

        if (type.IsClass) {
            return type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) is not null;
        }

        var printMembers = type.GetMethod(
            "PrintMembers",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(StringBuilder)],
            null
        );
        return printMembers?.GetCustomAttribute<CompilerGeneratedAttribute>() is not null;
    }
}
