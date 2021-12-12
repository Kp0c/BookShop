using System.Linq.Expressions;

namespace BookShop.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    void Add(T entity);
    T? FirstOrDefault(Expression<Func<T, bool>> filter);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}