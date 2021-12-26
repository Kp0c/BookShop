using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShopWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
    }
        
    public ActionResult Index()
    {
        return View();
    }

    public ActionResult Upsert(int? id)
    {
        var productViewModel = new ProductViewModel()
        {
            Product = new Product(),
            CategoryList = _unitOfWork.Category.GetAll()
                .Select(category => new SelectListItem(
                        category.Name,
                        category.Id.ToString()
                    )
                ),
            CoverTypeList = _unitOfWork.CoverType.GetAll()
                .Select(coverType => new SelectListItem(
                        coverType.Name,
                        coverType.Id.ToString()
                    )
                )
        };

        if (id is not (null or 0))
        {
            var product = _unitOfWork.Product.FirstOrDefault(product => product.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            productViewModel.Product = product;
        }

        return View(productViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Upsert(ProductViewModel productViewModel, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            return View(productViewModel);
        }

        if (file != null)
        {
            var wwwRootPath = _hostEnvironment.WebRootPath;
            var fileName = Guid.NewGuid().ToString();
            var uploads = Path.Combine(wwwRootPath, @"images\products");
            var extension = Path.GetExtension(file.FileName);

            if (productViewModel.Product.ImageUrl is not null)
            {
                var oldImagePath = Path.Combine(wwwRootPath, productViewModel.Product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            productViewModel.Product.ImageUrl = @$"\images\products\{fileName}{extension}";
        }

        if (productViewModel.Product.Id is 0)
        {
            _unitOfWork.Product.Add(productViewModel.Product);    
        }
        else
        {
            _unitOfWork.Product.Update(productViewModel.Product);
        }
            
        _unitOfWork.Save();

        TempData["success"] = "Product added successfully";

        return RedirectToAction("Index");
    }

    #region API Controller

    [HttpGet]
    public IActionResult GetAll()
    {
        var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

        return Ok(new
        {
            data = productList
        });
    }
        
    [HttpDelete]
    public ActionResult Delete(int? id)
    {
        var product = _unitOfWork.Product.FirstOrDefault(cat => cat.Id == id);

        if (product == null)
        {
            return NotFound();
        }


        Debug.Assert(product.ImageUrl != null, "product.ImageUrl != null");
        var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }
        _unitOfWork.Product.Remove(product);
        _unitOfWork.Save();

        TempData["success"] = "Cover Type removed successfully";

        return Ok();
    }

    #endregion
}