using SinovadMediaServer.Transversal.Collection;
using System.Linq.Expressions;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        TEntity Get(int id);
        Task<TEntity> GetAsync(int id, CancellationToken cancellationToken = default);
        TEntity GetByExpression(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        IEnumerable<TEntity> GetAll();
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        IEnumerable<TEntity> GetAllByExpression(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllByExpressionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<DataCollection<TEntity>> GetAllWithPaginationAsync(int page, int take, string sortBy, string sortDirection, string searchText=null, string searchBy=null, CancellationToken cancellationToken = default);
        Task<DataCollection<TEntity>> GetAllWithPaginationByExpressionAsync(int page, int take, string sortBy, string sortDirection, string searchText, string searchBy, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        TEntity Add(TEntity data);
        Task<TEntity> AddAsync(TEntity data, CancellationToken cancellationToken = default);
        void AddList(List<TEntity> list);
        Task AddListAsync(List<TEntity> list, CancellationToken cancellationToken = default);
        void Delete(int id);
        void DeleteList(List<TEntity> list);
        void DeleteByExpression(Expression<Func<TEntity, bool>> predicate);
        void Update(TEntity data);
        void Save();

    }
}
