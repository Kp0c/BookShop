using BookShop.DataAccess.Data;

namespace BookShop.DataAccess.Repository.IRepository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Category = new CategoryRepository(db);
        CoverType = new CoverTypeRepository(db);
        Product = new ProductRepository(db);
        Company = new CompanyRepository(db);
        ShoppingCart = new ShoppingCartRepository(db);
        ApplicationUser = new ApplicationUserRepository(db);
        OrderHeader = new OrderHeaderRepository(db);
        OrderDetail = new OrderDetailRepository(db);
    }

    public ICategoryRepository Category { get; }
    public ICoverTypeRepository CoverType { get; }
    public IProductRepository Product { get; }
    public ICompanyRepository Company { get; }
    public IShoppingCartRepository ShoppingCart { get; }
    public IApplicationUserRepository ApplicationUser { get; }
    public IOrderHeaderRepository OrderHeader { get; }
    public IOrderDetailRepository OrderDetail { get; }

    public void Save()
    {
        _db.SaveChanges();
    }
}