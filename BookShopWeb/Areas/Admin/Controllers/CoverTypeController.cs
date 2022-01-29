using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CoverTypeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CoverTypeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
        
    public ActionResult Index()
    {
        var coverTypes = _unitOfWork.CoverType.GetAll();
        return View(coverTypes);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(CoverType coverType)
    {
        if (!ModelState.IsValid)
        {
            return View(coverType);
        }

        _unitOfWork.CoverType.Add(coverType);
        _unitOfWork.Save();

        TempData["success"] = "Cover Type added successfully";

        return RedirectToAction("Index");
    }

    public ActionResult Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var coverType = _unitOfWork.CoverType.FirstOrDefault(coverType => coverType.Id == id);
        if (coverType == null)
        {
            return NotFound();
        }
            
        return View(coverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(CoverType coverType)
    {
        if (!ModelState.IsValid)
        {
            return View(coverType);
        }

        _unitOfWork.CoverType.Update(coverType);
        _unitOfWork.Save();

        TempData["success"] = "Cover Type updated successfully";

        return RedirectToAction("Index");
    }

    // GET: CoverType/Delete/5
    public ActionResult Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var coverType = _unitOfWork.CoverType.FirstOrDefault(coverType => coverType.Id == id);
        if (coverType == null)
        {
            return NotFound();
        }
            
        return View(coverType);
    }

    // POST: CoverType/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id)
    {
        var coverType = _unitOfWork.CoverType.FirstOrDefault(cat => cat.Id == id);

        if (coverType == null)
        {
            return NotFound();
        }
        _unitOfWork.CoverType.Remove(coverType);
        _unitOfWork.Save();

        TempData["success"] = "Cover Type removed successfully";

        return RedirectToAction("Index");
    }
}