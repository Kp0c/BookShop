using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;

namespace BookShop.DataAccess.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _db;
    
    public ProductRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Product product)
    {
        var fromDb = _db.Products.FirstOrDefault(p => p.Id == product.Id);

        if (fromDb == null) return;

        fromDb.Title = product.Title;
        fromDb.Isbn = product.Isbn;
        fromDb.Price = product.Price;
        fromDb.Price50 = product.Price50;
        fromDb.Price100 = product.Price100;
        fromDb.ListPrice = product.ListPrice;
        fromDb.Description = product.Description;
        fromDb.CategoryId = product.CategoryId;
        fromDb.Author = product.Author;
        fromDb.CoverTypeId = product.CoverTypeId;

        if (product.ImageUrl != null)
        {
            fromDb.ImageUrl = product.ImageUrl;
        }
    }
}