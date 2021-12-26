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

    public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
    {
        var query = filter == null ? _dbSet : _dbSet.Where(filter);
        
        if (includeProperties == null) return query.ToList();

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
        IQueryable<T> query = _dbSet.Where(filter);

        if (includeProperties == null) return _dbSet.FirstOrDefault(filter); 
        
        query = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(query, (current, includeProp) => current.Include(includeProp));
        
        return query.FirstOrDefault(filter);
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