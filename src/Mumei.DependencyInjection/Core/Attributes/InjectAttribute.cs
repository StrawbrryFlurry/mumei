﻿namespace Mumei.Core.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute : Attribute {
  public InjectAttribute() { }

  public InjectAttribute(string providerName) {
    ProviderName = providerName;
  }

  public string? ProviderName { get; }
}