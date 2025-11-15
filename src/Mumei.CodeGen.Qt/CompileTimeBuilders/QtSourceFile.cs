using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt;

public readonly struct QtSourceFile {
    public required string Name { get; init; }
    private readonly QtCollection<QtNamespace> _namespaceDeclarations = QtCollection<QtNamespace>.Empty;

    public QtSourceFile() { }

    [SetsRequiredMembers]
    public QtSourceFile(string name) {
        Name = name;
    }

    [SetsRequiredMembers]
    public QtSourceFile(string name, QtCollection<QtNamespace> namespaceDeclarations) {
        Name = name;
        _namespaceDeclarations = namespaceDeclarations;
    }

    public static QtSourceFile CreateObfuscated(string nameHint) {
        return new QtSourceFile(RandomNameGenerator.GenerateName(nameHint));
    }

    public QtSourceFile WithNamespace(QtNamespace ns) {
        return new QtSourceFile(Name, _namespaceDeclarations.Add(ns));
    }

    public void Render(IRenderTreeBuilder renderTreeBuilder) {
        foreach (var namespaceDeclaration in _namespaceDeclarations) {
            renderTreeBuilder.Node(namespaceDeclaration);
            renderTreeBuilder.NewLine();
        }
    }
}

/// <summary>
/// Describes a feature that needs to be available in the generated
/// source file in order for the generated code to work.
///
/// E.g. The InterceptsLocationAttribute must be defined in order
/// for interception to work.
/// </summary>
internal interface ISourceFileFeature : IRenderFragment;

/// <summary>
/// Describes a feature that needs to be available in the generated
/// source file in order for the generated code to work. These features
/// are accessible by all other source code and should only be generated
/// once per compilation unit.
/// </summary>
internal interface ICompilationFeature : IRenderFragment;

internal sealed class CodeGenFeatureCollection {
    private HashSet<ISourceFileFeature>? _sourceFileFeatures;
    private HashSet<ICompilationFeature>? _compilationUnitFeatures;

    public void Require(ISourceFileFeature feature) {
        (_sourceFileFeatures ??= []).Add(feature);
    }

    public void Require(ICompilationFeature feature) {
        (_compilationUnitFeatures ??= []).Add(feature);
    }

    public void RenderSourceFileFeatures(IRenderTreeBuilder renderTree) {
        if (_sourceFileFeatures is null) {
            return;
        }

        var i = _sourceFileFeatures.Count;
        foreach (var feature in _sourceFileFeatures) {
            renderTree.Node(feature);
            if (--i != 0) {
                renderTree.NewLine();
            }
        }
    }
}

internal static class CodeGenFeature {
    public static readonly MethodReflectionImpl MethodReflection = new();
    public static readonly InterceptorsImpl Interceptors = new();

    public sealed class InterceptorsImpl : ISourceFileFeature, IRenderer.IFeature {
        public void Render(IRenderTreeBuilder renderTree) {
            renderTree.Line("#pragma warning disable");
            renderTree.Node(NamespaceFragment.Create("System.Runtime.CompilerServices", [
                ClassDeclarationFragment.Create(
                    "InterceptsLocationAttribute",
                    accessModifier: AccessModifier.FileSealed,
                    attributes: [
                        AttributeFragment.Create(
                            typeof(AttributeUsageAttribute),
                            [
                                $"{typeof(AttributeTargets)}.Method",
                                AttributePropertyArgumentFragment.Create("AllowMultiple", "true")
                            ]
                        )
                    ],
                    primaryConstructorParameters: [
                        (typeof(int), "version"),
                        (typeof(string), "data")
                    ],
                    baseTypes: [typeof(Attribute)]
                )
            ]));
            renderTree.Line("#pragma warning enable");
        }
    }

    public sealed class MethodReflectionImpl : ICompilationFeature {
        private const string ClassIdentifier = "λMumeiMethodReflector";

        public void ReflectMethodInfo<TSyntaxWriter>(
            ref TSyntaxWriter writer,
            IQtType declaringType,
            string methodName,
            QtCollection<IQtType> typeParameters,
            QtCollection<IQtType> parameterTypes
        ) where TSyntaxWriter : ISyntaxWriter {
            writer.WriteFormatted($"{ClassIdentifier}.GetMethodInfo(");
            var typeParametersArray = typeParameters.RepresentAsQtArray((IQtType x) => x);
            var parameterTypesArray = parameterTypes.RepresentAsQtArray((IQtType x) => x);
            writer.WriteFormatted($"{declaringType:t}, {methodName:q}, {typeParametersArray:t}, {parameterTypesArray:t}");
            writer.Write(")");
        }

        public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
            writer.WriteFormattedBlock(
                // TODO: This implementation might collide with other auto-generated code
                // if multiple source generators are used in the same project.
                $$"""
                  internal static class {{ClassIdentifier}} {
                      public static {{typeof(MethodInfo):g}} GetMethodInfo(
                          {{typeof(Type):g}} declaringType,
                          string methodName,
                          {{typeof(Type):g}}[] typeParameters,
                          {{typeof(Type):g}}[] parameterTypes
                      ) {
                          var methods = declaringType.GetMethods({{typeof(BindingFlags):g}}.Public | {{typeof(BindingFlags):g}}.Static | {{typeof(BindingFlags):g}}.Instance | {{typeof(BindingFlags):g}}.NonPublic);
                          if (typeParameters.Length != 0) {
                              return GetMethodCore<GenericMatchingStrategy>(methodName, typeParameters, parameterTypes, methods);
                          }

                          return GetMethodCore<NonGenericMatchingStrategy>(methodName, typeParameters, parameterTypes, methods);
                      }

                      [{{typeof(MethodImplAttribute):g}}({{typeof(MethodImplOptions):g}}.AggressiveInlining)]
                      private static {{typeof(MethodInfo):g}} GetMethodCore<TMatchingStrategy>(
                          string methodName,
                          {{typeof(Type):g}}[] typeParameters,
                          {{typeof(Type):g}}[] parameterTypes,
                          {{typeof(MethodInfo):g}}[] methods
                      ) where TMatchingStrategy : struct, IMatchingStrategy {
                          foreach (var method in methods) {
                              if (MatchMethodCore(method, methodName, typeParameters, parameterTypes) is not { } matchedMethod) {
                                  continue;
                              }

                              if (typeof(TMatchingStrategy) == typeof(GenericMatchingStrategy) && matchedMethod.IsGenericMethodDefinition) {
                                  matchedMethod = matchedMethod.MakeGenericMethod(typeParameters);
                              }

                              return matchedMethod;
                          }

                          throw new {{typeof(MissingMethodException):g}}($"Method {methodName} with {typeParameters} type parameters and {parameterTypes.Length} parameters not found.");

                          static {{typeof(MethodInfo):g}}? MatchMethodCore(
                              {{typeof(MethodInfo):g}} method,
                              string methodName,
                              {{typeof(Type):g}}[] typeParameters,
                              {{typeof(Type):g}}[] parameterTypes
                          ) {
                              if (method.Name != methodName) {
                                  return null;
                              }

                              var isGenericMethod = method.IsGenericMethodDefinition;
                              if (typeof(TMatchingStrategy) == typeof(NonGenericMatchingStrategy) && method.IsGenericMethodDefinition) {
                                  return null;
                              }

                              if (typeof(TMatchingStrategy) == typeof(GenericMatchingStrategy) && isGenericMethod) {
                                  var genericArguments = method.GetGenericArguments();
                                  if (genericArguments.Length != typeParameters.Length) {
                                      return null;
                                  }
                              }

                              var parameters = method.GetParameters();
                              if (parameters.Length != parameterTypes.Length) {
                                  return null;
                              }

                              var match = method;
                              for (var i = 0; i < parameters.Length; i++) {
                                  var sourceType = parameters[i].ParameterType;
                                  var targetType = parameterTypes[i];
                                  if (sourceType == targetType) {
                                      continue;
                                  }

                                  if (typeof(TMatchingStrategy) != typeof(GenericMatchingStrategy)) {
                                      continue;
                                  }

                                  if (
                                      sourceType.IsGenericType
                                      && targetType.IsGenericType
                                      && sourceType.GetGenericTypeDefinition() == targetType.GetGenericTypeDefinition()
                                  ) {
                                      continue;
                                  }

                                  match = null;
                                  break;
                              }

                              return match;
                          }
                      }

                      private interface IMatchingStrategy;
                      private readonly struct NonGenericMatchingStrategy : IMatchingStrategy;
                      private readonly struct GenericMatchingStrategy : IMatchingStrategy;
                  }
                  """
            );
        }

        public void Render(IRenderTreeBuilder renderTree) {
            throw new NotImplementedException();
        }
    }
}