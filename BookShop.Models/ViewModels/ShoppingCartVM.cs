namespace BookShop.Models.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart> ListCart { get; set; }

    public double Total => ListCart.Sum(cart => cart.Price * cart.Count);
}