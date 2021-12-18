using System.Diagnostics;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Customer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var products = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

        return View(products);
    }

    public IActionResult Details(int id)
    {
        var product =
            _unitOfWork.Product.FirstOrDefault(product => product.Id == id, includeProperties: "Category,CoverType");

        if (product == null)
        {
            return RedirectToAction("Index");
        }

        ShoppingCart cart = new()
        {
            Count = 1,
            Product = product
        };

        return View(cart);
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