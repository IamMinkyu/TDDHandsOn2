using Microsoft.EntityFrameworkCore;

namespace Orders;

public static class QueryExtensions
{
    public static Task<Order?> FindOrder(this DbSet<Order> source, Guid id)
        => source.SingleOrDefaultAsync(x => x.Id == id);

    public static IQueryable<Order> FilterByUserId(this IQueryable<Order> source, Guid? userId)
    {
        if (userId == null) return source;
        return source.Where(x => x.UserId == userId);
    }

    public static IQueryable<Order> FilterByShopId(this IQueryable<Order> source, Guid? shopId)
    {
        if (shopId == null) return source;
        return source.Where(x => x.ShopId == shopId);   
    }
}
