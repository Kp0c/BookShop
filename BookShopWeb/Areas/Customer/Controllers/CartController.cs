using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookShopWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
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

        var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(cart => cart.ApplicationUserId == claim.Value,
            includeProperties: "Product").ToList();
        ShoppingCartVm = new ShoppingCartVM
        {
            ListCart = shoppingCarts,
            OrderHeader = new OrderHeader
            {
                OrderTotal = shoppingCarts.Sum(cart => cart.Price * cart.Count)
            }
        };

        return View(ShoppingCartVm);
    }

    public IActionResult Summary()
    {
        Debug.Assert(User != null, nameof(User) + " != null");
        Debug.Assert(User.Identity != null, "User.Identity != null");
        
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        
        if (claim?.Value == null)
        {
            return Unauthorized();
        }

        var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(cart => cart.ApplicationUserId == claim.Value,
            includeProperties: "Product").ToList();
        var user = _unitOfWork.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value)
                   ?? throw new ApplicationException("User not found");
        ShoppingCartVm = new ShoppingCartVM
        {
            ListCart = shoppingCarts,
            OrderHeader = new OrderHeader
            {
                OrderTotal = shoppingCarts.Sum(cart => cart.Price * cart.Count),
                ApplicationUser = user,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                StreetAddress = user.StreetAddress ?? string.Empty,
                City = user.City ?? string.Empty,
                State = user.State ?? string.Empty,
                PostalCode = user.PostalCode ?? string.Empty
            }
        };

        return View(ShoppingCartVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public IActionResult SummaryPost()
    {
        Debug.Assert(User != null, nameof(User) + " != null");
        Debug.Assert(User.Identity != null, "User.Identity != null");
        
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        
        if (claim?.Value == null)
        {
            return Unauthorized();
        }

        var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(cart => cart.ApplicationUserId == claim.Value,
            includeProperties: "Product").ToList();
        ShoppingCartVm.ListCart = shoppingCarts;
        
        ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVm.OrderHeader.ApplicationUserId = claim.Value;
        ShoppingCartVm.OrderHeader.OrderTotal = shoppingCarts.Sum(cart => cart.Price * cart.Count);

        var applicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value);

        if (applicationUser is null) return BadRequest("User not found");
        
        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            ShoppingCartVm.OrderHeader.PaymentStatus = PaymentStatus.Pending.ToString();
            ShoppingCartVm.OrderHeader.OrderStatus = Status.Pending.ToString();
        }
        else
        {
            ShoppingCartVm.OrderHeader.PaymentStatus = PaymentStatus.DelayedPayment.ToString();
            ShoppingCartVm.OrderHeader.OrderStatus = Status.Approved.ToString();
        }
        
        _unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
        _unitOfWork.Save();

        foreach (var cart in shoppingCarts)
        {
            var orderDetail = new OrderDetail()
            {
                ProductId = cart.ProductId,
                OrderId = ShoppingCartVm.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            var domain = "https://localhost:7272/";
            var options = new SessionCreateOptions()
            {
                LineItems = ShoppingCartVm.ListCart.Select((cart) => new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long) (cart.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = cart.Product.Title
                        }
                    },
                    Quantity = cart.Count
                }).ToList(),
                Mode = "payment",
                SuccessUrl = $"{domain}customer/cart/OrderConfirmation?id={ShoppingCartVm.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
            };

            var service = new SessionService();
            var session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripeStatus(ShoppingCartVm.OrderHeader.Id, session.Id,
                session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        else
        {
            return RedirectToAction("OrderConfirmation", "Cart", new
            {
                id = ShoppingCartVm.OrderHeader.Id
            });
        }
    }

    public IActionResult OrderConfirmation(int id)
    {
        var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(o => o.Id == id);

        if (orderHeader is null) return NotFound();

        if (orderHeader.PaymentStatus != PaymentStatus.DelayedPayment.ToString())
        {
            var service = new SessionService();
            var session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Status.Approved.ToString(),
                    PaymentStatus.Approved.ToString());
                _unitOfWork.Save();
            }
        }

        var shoppingCarts = _unitOfWork.ShoppingCart
            .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();
        return View(id);
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