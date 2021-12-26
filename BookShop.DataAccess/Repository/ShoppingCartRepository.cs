using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;

namespace BookShop.DataAccess.Repository;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private readonly ApplicationDbContext _db;
    public ShoppingCartRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void IncrementCount(ShoppingCart shoppingCart, int count)
    {
        shoppingCart.Count += count;
    }

    public void DecrementCount(ShoppingCart shoppingCart, int count)
    {
        shoppingCart.Count -= count;
    }
}