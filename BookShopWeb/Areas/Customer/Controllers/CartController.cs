using System;
using System.Diagnostics;
using System.Security.Claims;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ShoppingCartVM ShoppingCartVm { get; set; }

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        Debug.Assert(User != null, nameof(User) + " != null");
        Debug.Assert(User.Identity != null, "User.Identity != null");

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            return Unauthorized();
        }

        ShoppingCartVm = new ShoppingCartVM
        {
            ListCart = _unitOfWork.ShoppingCart.GetAll(cart => cart.ApplicationUserId == claim.Value,
                includeProperties: "Product")
        };

        return View(ShoppingCartVm);
    }

    public IActionResult Summary()
    {
        // Debug.Assert(User != null, nameof(User) + " != null");
        // Debug.Assert(User.Identity != null, "User.Identity != null");
        //
        // var claimsIdentity = (ClaimsIdentity)User.Identity;
        // var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //
        // if (claim == null)
        // {
        //     return Unauthorized();
        // }
        //
        // ShoppingCartVm = new ShoppingCartVM
        // {
        //     ListCart = _unitOfWork.ShoppingCart.GetAll(cart => cart.ApplicationUserId == claim.Value,
        //         includeProperties: "Product")
        // };

        return View();
    }

    public IActionResult Plus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.FirstOrDefault(cart => cart.Id == cartId);

        if (cart == null)
        {
            return BadRequest();
        }
        
        _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
        _unitOfWork.Save();

        return RedirectToAction("Index");
    }

    public IActionResult Minus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.FirstOrDefault(cart => cart.Id == cartId);

        if (cart == null)
        {
            return BadRequest();
        }

        if (cart.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Remove(cart);
        }
        else
        {
            _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
        }

        _unitOfWork.Save();
        
        return RedirectToAction("Index");
    }

    public IActionResult Remove(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.FirstOrDefault(cart => cart.Id == cartId);

        if (cart == null)
        {
            return BadRequest();
        }
        
        _unitOfWork.ShoppingCart.Remove(cart);
        _unitOfWork.Save();
        
        return RedirectToAction("Index");
    }
}