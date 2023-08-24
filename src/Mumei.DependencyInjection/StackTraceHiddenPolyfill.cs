#pragma warning disable
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace System.Diagnostics;

/// <summary>
/// Types and Methods attributed with StackTraceHidden will be omitted from the stack trace text shown in StackTrace.ToString() and Exception.StackTrace
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
  AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Struct,
  Inherited = false
)]
internal sealed class StackTraceHiddenAttribute : Attribute {
  /// <summary>
  /// Initializes a new instance of the <see cref="StackTraceHiddenAttribute"/> class.
  /// </summary>
  public StackTraceHiddenAttribute() { }
}