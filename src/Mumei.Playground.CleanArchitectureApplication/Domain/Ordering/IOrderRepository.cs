namespace CleanArchitectureApplication.Domain.Ordering;

public interface IOrderRepository {
  public void Add(Order order);
}