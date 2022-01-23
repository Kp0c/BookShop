using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;

namespace BookShop.DataAccess.Repository;

public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
    private readonly ApplicationDbContext _db;
    public OrderDetailRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderDetail orderDetail)
    {
        _db.OrderDetails.Update(orderDetail);
    }
}