﻿namespace Mumei.Core.Attributes;

public class Singleton : Attribute { }

public class SingletonAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }