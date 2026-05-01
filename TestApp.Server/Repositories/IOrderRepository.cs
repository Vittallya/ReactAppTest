namespace TestApp.Server.Repositories;

public interface IOrderRepository
{
    Task CreateOrder(Order order, CancellationToken token);

    Task<Order?> FindOrder(int id, CancellationToken token);

    Task<(IReadOnlyCollection<Order> data, bool hasMore)> GetOrders(int? lastId, int limit, CancellationToken token);
}
