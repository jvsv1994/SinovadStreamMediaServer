using Microsoft.EntityFrameworkCore;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Persistence.Contexts;
using SinovadMediaServer.Transversal.Collection;
using SinovadMediaServer.Transversal.Pagging;
using System.Linq.Expressions;

#nullable disable

namespace SinovadMediaServer.Persistence.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private DbSet<TEntity> _table = null;
        private ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _table = _context.Set<TEntity>();
        }
        public TEntity Get(int id)
        {
            var res = _table.Find(id);
            if (res != null)
            {
                return res;
            }
            throw new Exception("Not Found");
        }

        public async Task<TEntity> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var res = await _table.FindAsync(id, cancellationToken);
            if (res != null)
            {
                return res;
            }
            throw new Exception("Not Found");
        }

        public TEntity GetByExpression(Expression<Func<TEntity, bool>> predicate)
        {
            return _table.FirstOrDefault(predicate);
        }

        public async Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _table.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _table.ToList();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _table.ToListAsync(cancellationToken);
        }
        public IEnumerable<TEntity> GetAllByExpression(Expression<Func<TEntity, bool>> predicate)
        {
            return _table.Where(predicate).ToList();
        }

        public async Task<IEnumerable<TEntity>> GetAllByExpressionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _table.Where(predicate).ToListAsync(cancellationToken);
        }

        public IEnumerable<TEntity> GetAllByFunctionAsync(Func<TEntity, bool> function)
        {
            return _table.Where(function);
        }

        public async Task<DataCollection<TEntity>> GetAllWithPaginationAsync(int page, int take, string sortBy, string sortDirection, string searchText, string searchBy, CancellationToken cancellationToken = default)
        {
            return await _table.AsNoTracking().GetPagedAsync(page, take, sortBy, sortDirection, searchText, searchBy, cancellationToken);
        }

        public async Task<DataCollection<TEntity>> GetAllWithPaginationByExpressionAsync(int page, int take, string sortBy, string sortDirection, string searchText, string searchBy, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _table.AsNoTracking().Where(predicate).GetPagedAsync(page, take, sortBy, sortDirection, searchText, searchBy, cancellationToken);
        }

        public TEntity Add(TEntity data)
        {
            var entry = _table.Add(data);
            return entry.Entity;
        }

        public async Task<TEntity> AddAsync(TEntity data, CancellationToken cancellationToken = default)
        {
            var entry = await _table.AddAsync(data, cancellationToken);
            return entry.Entity;
        }


        public void AddList(List<TEntity> list)
        {
            _table.AddRange(list);
        }

        public async Task AddListAsync(List<TEntity> list, CancellationToken cancellationToken = default)
        {
            await _table.AddRangeAsync(list, cancellationToken);
        }

        public void Delete(int id)
        {
            var dataToDelete = _context.Set<TEntity>().FindAsync(id).Result;
            _table.Remove(dataToDelete);
        }

        public void DeleteList(List<TEntity> list)
        {
            _table.RemoveRange(list);
        }

        public void DeleteByExpression(Expression<Func<TEntity, bool>> predicate)
        {
            _table.RemoveRange(_table.Where(predicate));
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(TEntity data)
        {
            _context.Attach(data);
            _context.Entry(data).State = EntityState.Modified;
        }

    }
}
