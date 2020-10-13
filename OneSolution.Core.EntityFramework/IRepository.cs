using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace OneSolution.Core.EntityFramework
{
    public interface IRepository<DbContext>
    {
        IQueryable<TEntity> AsQueryable<TEntity>()
            where TEntity : class;

        IQueryable<TEntity> AsQueryable<TEntity>(IPredicateQuery<TEntity> query)
            where TEntity : class;

        IQueryable<TEntity> AsQueryable<TEntity>(ILinqQuery<TEntity> query)
            where TEntity : class;

        IAsyncEnumerable<TEntity> AsAsyncEnumerable<TEntity>() 
            where TEntity : class;

        IAsyncEnumerable<TEntity> AsAsyncEnumerable<TEntity>(IPredicateQuery<TEntity> query) 
            where TEntity : class;

        int ExecuteSqlCommand(string sql, params object[] parameters);

        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);

        void Commit();

        void DiscardChanges();

        void Dispose();

        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        TEntity Create<TEntity>() where TEntity : class;

        TEntity Insert<TEntity>(TEntity T) where TEntity : class;

        void InsertRange<TEntity>(IEnumerable<TEntity> entitys) where TEntity : class;

        TEntity Update<TEntity>(TEntity T) where TEntity : class;

        TEntity Delete<TEntity>(TEntity T) where TEntity : class;

        void DeleteRange<TEntity>(IEnumerable<TEntity> entitys) where TEntity : class;

        void Detach<TEntity>(TEntity entity) where TEntity : class;

        int SaveChanges();

        Task<int> SaveChangesAsync();

        IDbConnection GetConnection();
    }
}