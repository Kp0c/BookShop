using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookShop.Models;

public class ShoppingCart
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product? Product { get; set; }

    [Range(1, 1000)]
    public int Count { get; set; }
    
    public string ApplicationUserId { get; set; }
    
    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; }

    [NotMapped] public double Price => Product == null ? 0 : GetPriceBasedOnQuantity(Count, Product.Price, Product.Price50, Product.Price100); 

    private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
    {
        return  quantity switch
        {
            <=50 => price,
            <=100 => price50,
            _ => price100
        };
    }
}