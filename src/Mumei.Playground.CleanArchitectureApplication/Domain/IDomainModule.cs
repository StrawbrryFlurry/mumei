using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.Domain;

[Import<IOrderComponent>]
public interface IDomainModule { }