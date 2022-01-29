using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Upsert(int? id)
    {
        Company? company = new();

        if (id is null or 0) return View(company);

        company = _unitOfWork.Company.FirstOrDefault(dbCompany => dbCompany.Id == id);

        if (company is null)
        {
            return NotFound();
        }

        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(Company company)
    {
        if (!ModelState.IsValid)
        {
            return View(company);
        }

        if (company.Id is 0)
        {
            _unitOfWork.Company.Add(company);

            TempData["success"] = "Company added successfully";
        }
        else
        {
            _unitOfWork.Company.Update(company);

            TempData["success"] = "Company updated successfully";
        }
        
        _unitOfWork.Save();

        return RedirectToAction("Index");
    }
    
    #region API Controller

    [HttpGet]
    public IActionResult GetAll()
    {
        var companies = _unitOfWork.Company.GetAll();

        return Ok(new
        {
            data = companies
        });
    }
        
    [HttpDelete]
    public ActionResult Delete(int? id)
    {
        var company = _unitOfWork.Company.FirstOrDefault(company => company.Id == id);

        if (company == null)
        {
            return NotFound();
        }
        
        _unitOfWork.Company.Remove(company);
        _unitOfWork.Save();

        TempData["success"] = "Company removed successfully";

        return Ok();
    }

    #endregion
}