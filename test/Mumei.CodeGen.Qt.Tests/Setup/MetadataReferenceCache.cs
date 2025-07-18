﻿using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Tests.Setup;

internal static class MetadataReferenceCache {
    public static (string AssemblyLocation, MetadataReference MetadataReference) SystemCoreLib { get; } = (
        typeof(Attribute).Assembly.Location,
        MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)
    );

    private static readonly Dictionary<Type, MetadataReference> ReferenceCache = new();

    public static MetadataReference GetReference<TAssemblyType>() {
        if (ReferenceCache.TryGetValue(typeof(TAssemblyType), out var reference)) {
            return reference;
        }

        var typeInfo = typeof(TAssemblyType).GetTypeInfo();
        var assemblyLocation = typeInfo.Assembly.Location;

        reference = MetadataReference.CreateFromFile(assemblyLocation);
        ReferenceCache.Add(typeof(TAssemblyType), reference);

        return reference;
    }
}