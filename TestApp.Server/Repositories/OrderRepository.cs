using LiteDB;
using Microsoft.EntityFrameworkCore;
using TestApp.Server.Infrastructure;

namespace TestApp.Server.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task CreateOrder(Order order, CancellationToken token)
    {
        db.Add(order);
        await db.SaveChangesAsync(token);
    }

    public async Task<Order?> FindOrder(int id, CancellationToken token)
    {
        return await db.Orders.FindAsync([id], cancellationToken: token);
    }
    public async Task<(IReadOnlyCollection<Order> data, bool hasMore)> GetOrders(int? lastId, int limit, CancellationToken token)
    {
        var query = db.Orders.AsQueryable();

        if(lastId != null)
        {
            query = query.Where(x => x.Id > lastId);
        }

        var list = await query.OrderBy(x => x.Id).Take(limit + 1).AsNoTracking().ToListAsync();

        var hasMore = list.Count == limit + 1;
        if(hasMore)
            list.RemoveAt(list.Count - 1);


        return (list, hasMore);
    }
}
