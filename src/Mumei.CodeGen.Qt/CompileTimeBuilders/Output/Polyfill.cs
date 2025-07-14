﻿// ReSharper disable once CheckNamespace

namespace System.Runtime.CompilerServices;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct,
    AllowMultiple = false,
    Inherited = false
)]
internal sealed class InterpolatedStringHandlerAttribute : Attribute;