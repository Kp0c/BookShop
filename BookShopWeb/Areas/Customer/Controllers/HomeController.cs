using System.Diagnostics;
using System.Security.Claims;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var products = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

        return View(products);
    }

    public IActionResult Details(int productId)
    {
        var product =
            _unitOfWork.Product.FirstOrDefault(product => product.Id == productId, includeProperties: "Category,CoverType");

        if (product == null)
        {
            return RedirectToAction("Index");
        }

        ShoppingCart cart = new()
        {
            Count = 1,
            ProductId = productId,
            Product = product
        };

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        Debug.Assert(User != null, nameof(User) + " != null");
        Debug.Assert(User.Identity != null, "User.Identity != null");

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        
        Debug.Assert(claim != null, nameof(claim) + " != null");
        shoppingCart.ApplicationUserId = claim.Value;

        var cartFromDb = _unitOfWork.ShoppingCart.FirstOrDefault(cart =>
            cart.ProductId == shoppingCart.ProductId &&
            cart.ApplicationUserId == claim.Value
        );

        if (cartFromDb is not null)
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
        }
        else
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
        }
        _unitOfWork.Save();


        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}