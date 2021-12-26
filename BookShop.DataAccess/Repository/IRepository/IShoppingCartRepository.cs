using BookShop.Models;

namespace BookShop.DataAccess.Repository.IRepository;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    void IncrementCount(ShoppingCart shoppingCart, int count);
    void DecrementCount(ShoppingCart shoppingCart, int count);
}