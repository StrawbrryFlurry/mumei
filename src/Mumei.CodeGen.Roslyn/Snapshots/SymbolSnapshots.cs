using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.CodeGen;

/// <summary>Turns Roslyn symbols into equatable models that are safe to pass through the incremental pipeline.</summary>
public static class SymbolSnapshots {
    private static readonly SymbolDisplayFormat FullNameFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
            SymbolDisplayMiscellaneousOptions.ExpandValueTuple
        );

    private static readonly SymbolDisplayFormat NamespaceFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
    );

    public static TypeRef ToTypeRef(this ITypeSymbol type) {
        var named = type as INamedTypeSymbol;
        var array = type as IArrayTypeSymbol;
        var elementType = array?.ElementType.ToTypeRef();

        return new TypeRef {
            FullName = type.ToDisplayString(FullNameFormat),
            Name = array is not null ? $"{elementType!.Name}[{new string(',', array.Rank - 1)}]" : type.Name,
            Namespace = GetNamespace(type),
            MetadataName = GetMetadataName(type),
            TypeArguments = named?.TypeArguments.Select(t => t.ToTypeRef()).ToEquatableArray() ?? EquatableArray<TypeRef>.Empty,
            ElementType = elementType,
            IsClass = type.TypeKind == TypeKind.Class,
            IsStruct = type.TypeKind == TypeKind.Struct,
            IsInterface = type.TypeKind == TypeKind.Interface,
            IsEnum = type.TypeKind == TypeKind.Enum,
            IsDelegate = type.TypeKind == TypeKind.Delegate,
            IsRecord = type.IsRecord,
            IsArray = array is not null,
            IsTypeParameter = type.TypeKind == TypeKind.TypeParameter
        };
    }

    public static TypeModel ToTypeModel(this INamedTypeSymbol type) {
        var members = type.GetMembers();
        var containingTypes = new List<TypeRef>();
        for (var containing = type.ContainingType; containing is not null; containing = containing.ContainingType) {
            containingTypes.Insert(0, containing.ToTypeRef());
        }

        return new TypeModel {
            Type = type.ToTypeRef(),
            Modifiers = GetModifiers(type),
            BaseType = type.BaseType?.ToTypeRef(),
            Interfaces = type.AllInterfaces.Select(i => i.ToTypeRef()).ToEquatableArray(),
            Attributes = type.GetAttributes().Select(a => a.ToAttributeModel()).ToEquatableArray(),
            Properties = members.OfType<IPropertySymbol>()
                .Where(p => !p.IsImplicitlyDeclared && !p.IsIndexer)
                .Select(p => p.ToPropertyModel())
                .ToEquatableArray(),
            Fields = members.OfType<IFieldSymbol>()
                .Where(f => !f.IsImplicitlyDeclared)
                .Select(f => f.ToFieldModel())
                .ToEquatableArray(),
            Methods = members.OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary && !m.IsImplicitlyDeclared)
                .Select(m => m.ToMethodModel())
                .ToEquatableArray(),
            Constructors = type.InstanceConstructors.Select(c => c.ToMethodModel()).ToEquatableArray(),
            ContainingTypes = containingTypes.ToEquatableArray(),
            Location = type.Locations.FirstOrDefault(l => l.IsInSource)?.ToSourceLocation()
        };
    }

    public static PropertyModel ToPropertyModel(this IPropertySymbol property) {
        return new PropertyModel(
            property.Name,
            property.Type.ToTypeRef(),
            GetModifiers(property),
            property.GetMethod is not null,
            property.SetMethod is not null,
            property.SetMethod?.IsInitOnly == true
        ) {
            Attributes = property.GetAttributes().Select(a => a.ToAttributeModel()).ToEquatableArray()
        };
    }

    public static FieldModel ToFieldModel(this IFieldSymbol field) {
        return new FieldModel(field.Name, field.Type.ToTypeRef(), GetModifiers(field)) {
            Attributes = field.GetAttributes().Select(a => a.ToAttributeModel()).ToEquatableArray(),
            ConstantValue = field.HasConstantValue ? field.ConstantValue : null
        };
    }

    /// <summary>
    /// Snapshots a method. Extension methods are always described in their static form
    /// (the receiver is the first parameter), even when invoked as <c>receiver.Method()</c>.
    /// </summary>
    public static MethodModel ToMethodModel(this IMethodSymbol method) {
        if (method.ReducedFrom is not null) {
            method = method.GetConstructedReducedFrom() ?? method;
        }

        return new MethodModel(
            method.MethodKind == MethodKind.Constructor ? method.ContainingType.Name : method.Name,
            method.ContainingType.ToTypeRef(),
            method.ReturnType.ToTypeRef(),
            method.Parameters.Select(p => p.ToParameterModel()).ToEquatableArray(),
            GetModifiers(method)
        ) {
            TypeArguments = method.TypeArguments.Select(t => t.ToTypeRef()).ToEquatableArray(),
            Attributes = method.GetAttributes().Select(a => a.ToAttributeModel()).ToEquatableArray(),
            IsExtension = method.IsExtensionMethod
        };
    }

    public static ParameterModel ToParameterModel(this IParameterSymbol parameter) {
        var modifiers = parameter.RefKind switch {
            RefKind.Ref => ParameterModifiers.Ref,
            RefKind.Out => ParameterModifiers.Out,
            RefKind.In => ParameterModifiers.In,
            RefKind.RefReadOnlyParameter => ParameterModifiers.Ref | ParameterModifiers.Readonly,
            _ => ParameterModifiers.None
        };

        if (parameter.IsParams) {
            modifiers |= ParameterModifiers.Params;
        }

        if (parameter.IsThis) {
            modifiers |= ParameterModifiers.This;
        }

        if (parameter.ScopedKind != ScopedKind.None) {
            modifiers |= ParameterModifiers.Scoped;
        }

        return new ParameterModel(parameter.Name, parameter.Type.ToTypeRef(), modifiers) {
            HasDefaultValue = parameter.HasExplicitDefaultValue,
            DefaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null
        };
    }

    public static AttributeModel ToAttributeModel(this AttributeData attribute) {
        return new AttributeModel(
            attribute.AttributeClass?.ToTypeRef() ?? TypeRef.Named("?"),
            attribute.ConstructorArguments.Select(ToValue).ToEquatableArray(),
            attribute.NamedArguments.Select(a => new NamedArgument(a.Key, ToValue(a.Value))).ToEquatableArray()
        );
    }

    public static SourceLocation ToSourceLocation(this Location location) {
        var lineSpan = location.GetLineSpan();
        return new SourceLocation(
            lineSpan.Path ?? "",
            location.SourceSpan.Start,
            location.SourceSpan.Length,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character
        );
    }

    public static Location ToLocation(this SourceLocation location) {
        return Location.Create(
            location.FilePath,
            new TextSpan(location.Start, location.Length),
            new LinePositionSpan(
                new LinePosition(location.StartLine, location.StartColumn),
                new LinePosition(location.EndLine, location.EndColumn)
            )
        );
    }

    /// <summary>The metadata name of the symbol's definition, e.g. <c>System.Collections.Generic.List`1</c>.</summary>
    public static string GetMetadataName(this ITypeSymbol type) {
        switch (type) {
            case IArrayTypeSymbol array:
                return $"{GetMetadataName(array.ElementType)}[{new string(',', array.Rank - 1)}]";
            case ITypeParameterSymbol:
                return type.Name;
        }

        if (type.ContainingType is not null) {
            return $"{GetMetadataName(type.ContainingType)}+{type.MetadataName}";
        }

        var ns = GetNamespace(type);
        return ns.Length == 0 ? type.MetadataName : $"{ns}.{type.MetadataName}";
    }

    internal static CallSite? ToCallSite(
        this InvocationExpressionSyntax invocation,
        IMethodSymbol method,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    ) {
#pragma warning disable RSEXPERIMENTAL002
        var interceptableLocation = semanticModel.GetInterceptableLocation(invocation, cancellationToken);
#pragma warning restore RSEXPERIMENTAL002
        if (interceptableLocation is null) {
            return null;
        }

        var nameLocation = GetInvokedNameSyntax(invocation.Expression)?.GetLocation() ?? invocation.GetLocation();
        return new CallSite(
            method.ToMethodModel(),
            new InterceptLocation(
                interceptableLocation.Version,
                interceptableLocation.Data,
                interceptableLocation.GetDisplayLocation()
            ),
            nameLocation.ToSourceLocation()
        );
    }

    internal static SimpleNameSyntax? GetInvokedNameSyntax(ExpressionSyntax expression) {
        return expression switch {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
            SimpleNameSyntax name => name,
            _ => null
        };
    }

    internal static Modifiers GetModifiers(ISymbol symbol) {
        var modifiers = symbol.DeclaredAccessibility switch {
            Accessibility.Public => Modifiers.Public,
            Accessibility.Private => Modifiers.Private,
            Accessibility.Protected => Modifiers.Protected,
            Accessibility.Internal => Modifiers.Internal,
            Accessibility.ProtectedOrInternal => Modifiers.Protected | Modifiers.Internal,
            Accessibility.ProtectedAndInternal => Modifiers.Private | Modifiers.Protected,
            _ => Modifiers.None
        };

        if (symbol.IsStatic) {
            modifiers |= Modifiers.Static;
        }

        if (symbol.IsAbstract) {
            modifiers |= Modifiers.Abstract;
        }

        if (symbol.IsVirtual) {
            modifiers |= Modifiers.Virtual;
        }

        if (symbol.IsOverride) {
            modifiers |= Modifiers.Override;
        }

        if (symbol.IsSealed) {
            modifiers |= Modifiers.Sealed;
        }

        if (symbol.IsExtern) {
            modifiers |= Modifiers.Extern;
        }

        switch (symbol) {
            case INamedTypeSymbol type:
                if (type.IsFileLocal) {
                    modifiers = (modifiers & ~Modifiers.Accessibility) | Modifiers.File;
                }

                if (type.IsReadOnly) {
                    modifiers |= Modifiers.Readonly;
                }

                if (type.IsRefLikeType) {
                    modifiers |= Modifiers.Ref;
                }

                if (IsPartial(type)) {
                    modifiers |= Modifiers.Partial;
                }

                break;
            case IFieldSymbol field:
                if (field.IsConst) {
                    modifiers = (modifiers & ~Modifiers.Static) | Modifiers.Const;
                }

                if (field.IsReadOnly) {
                    modifiers |= Modifiers.Readonly;
                }

                if (field.IsVolatile) {
                    modifiers |= Modifiers.Volatile;
                }

                if (field.IsRequired) {
                    modifiers |= Modifiers.Required;
                }

                break;
            case IPropertySymbol { IsRequired: true }:
                modifiers |= Modifiers.Required;
                break;
            case IMethodSymbol method:
                if (method.IsAsync) {
                    modifiers |= Modifiers.Async;
                }

                if (method.IsReadOnly) {
                    modifiers |= Modifiers.Readonly;
                }

                if (method.IsPartialDefinition || method.PartialDefinitionPart is not null) {
                    modifiers |= Modifiers.Partial;
                }

                break;
        }

        return modifiers;
    }

    private static bool IsPartial(INamedTypeSymbol type) {
        foreach (var reference in type.DeclaringSyntaxReferences) {
            if (reference.GetSyntax() is TypeDeclarationSyntax declaration
                && declaration.Modifiers.Any(SyntaxKind.PartialKeyword)) {
                return true;
            }
        }

        return false;
    }

    private static string GetNamespace(ITypeSymbol type) {
        if (type is IArrayTypeSymbol array) {
            return GetNamespace(array.ElementType);
        }

        var ns = type.ContainingNamespace;
        return ns is null || ns.IsGlobalNamespace ? "" : ns.ToDisplayString(NamespaceFormat);
    }

    private static object? ToValue(TypedConstant constant) {
        return constant.Kind switch {
            TypedConstantKind.Array => constant.Values.IsDefault ? null : (object) constant.Values.Select(ToValue).ToEquatableArray(),
            TypedConstantKind.Type => (constant.Value as ITypeSymbol)?.ToTypeRef(),
            TypedConstantKind.Error => null,
            _ => constant.Value
        };
    }
}
