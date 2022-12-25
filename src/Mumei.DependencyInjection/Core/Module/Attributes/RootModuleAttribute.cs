﻿namespace Mumei.DependencyInjection.Core;

/// <summary>
///   Declares the interface as the root module for an application environment.
///   This attribute can be omitted, if the root module can be determined statically,
///   for example by calling PlatformInjector.CreateEnvironment{TRootModule} in the application startup.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class RootModuleAttribute : Attribute { }