using BookShop.Models;

namespace BookShop.DataAccess.Repository.IRepository;

public interface ICoverTypeRepository : IRepository<CoverType>
{
    void Update(CoverType coverType);
}