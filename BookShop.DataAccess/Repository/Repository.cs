using System.Linq.Expressions;
using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookShop.DataAccess.Repository;

public class Repository<T> : IRepository<T> where T: class
{
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext db)
    {
        _dbSet = db.Set<T>();
    }

    public IEnumerable<T> GetAll(string? includeProperties = null)
    {
        if (includeProperties == null) return _dbSet.ToList();

        IQueryable<T> query = _dbSet;
        query = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(query, (current, includeProp) => current.Include(includeProp));

        return query.ToList();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }
    public T? FirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
    {
        return _dbSet.FirstOrDefault(filter);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}