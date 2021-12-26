using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;

namespace BookShop.DataAccess.Repository;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    private ApplicationDbContext _db;
    
    public CompanyRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Company company)
    {
        var fromDb = _db.Companies.FirstOrDefault(p => p.Id == company.Id);

        if (fromDb == null) return;

        fromDb.Name = company.Name;
        fromDb.City = company.City;
        fromDb.PhoneNumber = company.PhoneNumber;
        fromDb.PostalCode = company.PostalCode;
        fromDb.StreetAddress = company.StreetAddress;
        company.State = company.State;
    }
}