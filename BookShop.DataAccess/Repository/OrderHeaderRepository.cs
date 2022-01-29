using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;

namespace BookShop.DataAccess.Repository;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderHeaderRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderHeader orderHeader)
    {
        _db.OrderHeaders.Update(orderHeader);
    }

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
        var orderFromDb = _db.OrderHeaders.FirstOrDefault(oh => oh.Id == id);

        if (orderFromDb == null) return;

        orderFromDb.OrderStatus = orderStatus;

        if (paymentStatus != null)
        {
            orderFromDb.PaymentStatus = paymentStatus;
        }
    }

    public void UpdateStripeStatus(int id, string sessionId, string paymentIntentId)
    {
        var orderFromDb = _db.OrderHeaders.FirstOrDefault(oh => oh.Id == id);

        if (orderFromDb == null) return;
        
        orderFromDb.PaymentDate = DateTime.Now;
        orderFromDb.SessionId = sessionId;
        orderFromDb.PaymentIntentId = paymentIntentId;
    }
}