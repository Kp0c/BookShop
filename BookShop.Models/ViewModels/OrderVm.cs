namespace BookShop.Models.ViewModels;

public class OrderVm
{
    public OrderHeader OrderHeader { get; set; }
    public IEnumerable<OrderDetail> OrderDetails { get; set; }
}