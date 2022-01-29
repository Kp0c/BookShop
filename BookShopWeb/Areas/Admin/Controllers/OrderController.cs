using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BookShopWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    
    [BindProperty]
    public OrderVm OrderVm { get; set; }

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int orderId)
    {
        OrderVm = new OrderVm()
        {
            OrderHeader =
                _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
        };

        return View(OrderVm);
    }

    [ActionName("Details")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DetailsPayNow()
    {
        OrderVm.OrderHeader = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id,
            includeProperties: "ApplicationUser");

        OrderVm.OrderDetails =
            _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVm.OrderHeader.Id, includeProperties: "Product");
        
        var domain = "https://localhost:7272/";
        var options = new SessionCreateOptions()
        {
            LineItems = OrderVm.OrderDetails.Select((cart) => new SessionLineItemOptions()
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
            SuccessUrl = $"{domain}admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
            CancelUrl = domain + $"admin/order/details?orderId={OrderVm.OrderHeader.Id}",
        };

        var service = new SessionService();
        var session = service.Create(options);

        _unitOfWork.OrderHeader.UpdateStripeStatus(OrderVm.OrderHeader.Id, session.Id,
            session.PaymentIntentId);
        _unitOfWork.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(o => o.Id == orderHeaderId);

        if (orderHeader is null) return NotFound();

        if (orderHeader.PaymentStatus == PaymentStatus.DelayedPayment.ToString())
        {
            var service = new SessionService();
            var session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus,
                    PaymentStatus.Approved.ToString());
                _unitOfWork.Save();
            }
        }
        return View(orderHeaderId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateOrderDetails()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id);
        if (orderHeaderFromDb == null) return BadRequest("Order not found");

        orderHeaderFromDb.Name = OrderVm.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = OrderVm.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = OrderVm.OrderHeader.City;
        orderHeaderFromDb.State = OrderVm.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVm.OrderHeader.PostalCode;

        if (OrderVm.OrderHeader.Carrier != null)
        {
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;
        }

        if (OrderVm.OrderHeader.TrackingNumber != null)
        {
            orderHeaderFromDb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        }

        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();

        TempData["Success"] = "Order Details Updated Successfully";

        return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    [ValidateAntiForgeryToken]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, Status.Processing.ToString());
        _unitOfWork.Save();

        TempData["Success"] = "Order Status Updated Successfully";

        return RedirectToAction("Details", "Order", new { orderId = OrderVm.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    [ValidateAntiForgeryToken]
    public IActionResult ShipOrder()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id);
        if (orderHeaderFromDb == null) return BadRequest("Order not found");

        orderHeaderFromDb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;
        orderHeaderFromDb.OrderStatus = Status.Shipped.ToString();
        orderHeaderFromDb.ShippingDate = DateTime.Now;

        if (orderHeaderFromDb.PaymentStatus == PaymentStatus.DelayedPayment.ToString())
        {
            orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
        }

        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();

        TempData["Success"] = "Order Shipped Successfully";

        return RedirectToAction("Details", "Order", new { orderId = OrderVm.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    [ValidateAntiForgeryToken]
    public IActionResult CancelOrder()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id);
        if (orderHeaderFromDb == null) return BadRequest("Order not found");

        if (orderHeaderFromDb.PaymentStatus == PaymentStatus.Approved.ToString())
        {
            var options = new RefundCreateOptions()
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeaderFromDb.PaymentIntentId,
            };

            var service = new RefundService();
            Refund refund = service.Create(options);
            
            _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, Status.Cancelled.ToString(), PaymentStatus.Refunded.ToString());
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, Status.Cancelled.ToString(), PaymentStatus.Cancelled.ToString());
        }
        _unitOfWork.Save();

        TempData["Success"] = "Order Cancelled Successfully";

        return RedirectToAction("Details", "Order", new { orderId = OrderVm.OrderHeader.Id });
    }

    #region API calls
    
    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;
        if (User.IsInRole(Roles.Admin.ToString()) || User.IsInRole(Roles.Employee.ToString()))
        {
            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");    
        }
        else
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "ApplicationUser");
        }
        
        

        switch (status)
        {
            case "pending":
                orderHeaders =
                    orderHeaders.Where(order => order.PaymentStatus == PaymentStatus.DelayedPayment.ToString());
                break;
            case "inProcess":
                orderHeaders =
                    orderHeaders.Where(order => order.OrderStatus == Status.Processing.ToString());
                break;
            case "completed":
                orderHeaders =
                    orderHeaders.Where(order => order.OrderStatus == Status.Shipped.ToString());
                break;
            case "approved":
                orderHeaders =
                    orderHeaders.Where(order => order.OrderStatus == Status.Approved.ToString());
                break;
        }

        return Ok(new
        {
            data = orderHeaders
        });
    }
    
    #endregion
}