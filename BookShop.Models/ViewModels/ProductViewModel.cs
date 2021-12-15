using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShop.Models.ViewModels;

public class ProductViewModel
{
    public Product Product { get; set; } = null!;

    [ValidateNever] 
    public IEnumerable<SelectListItem> CategoryList { get; init; } = null!;
    [ValidateNever]
    public IEnumerable<SelectListItem> CoverTypeList { get; init; } = null!;

}