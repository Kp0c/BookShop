using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers;

public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET
    public IActionResult Index()
    {
        var categoryList = _unitOfWork.Category.GetAll();
        return View(categoryList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        
        _unitOfWork.Category.Add(category);
        _unitOfWork.Save();
        TempData["success"] = "Category created successfully";
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int? id)
    {
        if (id is null or 0)
        {
            return NotFound();
        }

        var categoryFromDb = _unitOfWork.Category.FirstOrDefault(cat => cat.Id == id);

        if (categoryFromDb is null)
        {
            return NotFound();
        }
        
        return View(categoryFromDb);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        
        _unitOfWork.Category.Update(category);
        _unitOfWork.Save();
        
        TempData["success"] = "Category updated successfully";
        
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int? id)
    {
        if (id is null or 0)
        {
            return NotFound();
        }

        var categoryFromDb = _unitOfWork.Category.FirstOrDefault(cat => cat.Id == id);

        if (categoryFromDb is null)
        {
            return NotFound();
        }
        
        return View(categoryFromDb);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var category = _unitOfWork.Category.FirstOrDefault(cat => cat.Id == id);

        if (category == null)
        {
            return NotFound();
        }
        
        _unitOfWork.Category.Remove(category);
        _unitOfWork.Save();
        
        TempData["success"] = "Category deleted successfully";
        
        return RedirectToAction("Index");
    }
}