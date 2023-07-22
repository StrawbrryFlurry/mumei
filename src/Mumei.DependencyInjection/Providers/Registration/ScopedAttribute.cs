﻿namespace Mumei.DependencyInjection.Providers.Registration;

public class ScopedAttribute<TProvider, TImplementation> :
  DependencyRegistrationAttribute<TProvider, TImplementation> where TImplementation : TProvider { }

public class ScopedAttribute<TProvider> : DependencyRegistrationAttribute<TProvider> { }

public class ScopedAttribute : Attribute { }