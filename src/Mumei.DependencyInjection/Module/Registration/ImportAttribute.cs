﻿namespace Mumei.DependencyInjection.Module.Registration;

/// <summary>
///   Imports the module specified by the decorated property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ImportAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class ImportAttribute<TModule> : Attribute { }