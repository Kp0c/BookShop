using System.ComponentModel.DataAnnotations;

namespace BookShop.Models;

public class ShoppingCart
{
    public Product Product { get; set; } = null!;

    [Range(1, 1000)]
    public int Count { get; set; }
}