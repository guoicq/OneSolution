using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OneSolution.Core.EntityFramework
{

    public class Repository<TDbContext> : IDisposable, IRepository<TDbContext> where TDbContext : DbContext
    {
        private TDbContext _context;
        private IDbContextTransaction _transaction;

        public Repository(TDbContext context)
        {
            _context = context ?? throw new ArgumentNullException("Parameter context can't be null.");
        }

        public IQueryable<TEntity> AsQueryable<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>();
        }

        public IQueryable<TEntity> AsQueryable<TEntity>(IPredicateQuery<TEntity> query) where TEntity : class
        {
            return _context.Set<TEntity>().Where(query.Predicate);
        }

        public IQueryable<TEntity> AsQueryable<TEntity>(ILinqQuery<TEntity> query) where TEntity : class
        {
            return AsQueryable(query as ILinqQuery<TEntity, TEntity>);
        }

        public IAsyncEnumerable<TEntity> AsAsyncEnumerable<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>().AsAsyncEnumerable();
        }

        public IAsyncEnumerable<TEntity> AsAsyncEnumerable<TEntity>(IPredicateQuery<TEntity> query) where TEntity : class
        {
            return _context.Set<TEntity>().Where(query.Predicate).AsAsyncEnumerable();
        }
        
        private IQueryable<TResultEntity> AsQueryable<TResultEntity, TEntity>(
            ILinqQuery<TResultEntity, TEntity> query)
            where TResultEntity : class
            where TEntity : class
        {
            query.DataSource = _context.Set<TEntity>();

            return query.LinqQuery;
        }

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch (Exception ex)
            {
                _transaction.Rollback();

                DBExceptionHelper.DbErrorHandler(ex);
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = _context.Database.BeginTransaction(isolationLevel);
            return _transaction;
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
            return _transaction;
        }

        public void DiscardChanges()
        {
            try
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlRaw(sql, parameters);
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public TEntity Create<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>().CreateProxy();
        }

        public TEntity Insert<TEntity>(TEntity entity) where TEntity : class
        {
            var entry = _context.Add(entity);
            return entry.Entity;
        }

        public void InsertRange<TEntity>(IEnumerable<TEntity> entitys) where TEntity : class
        {
            _context.AddRange(entitys);
        }

        public TEntity Update<TEntity>(TEntity entity) where TEntity : class
        {
            var entry = _context.Update(entity);
            return entry.Entity;
        }

        public TEntity Delete<TEntity>(TEntity entity) where TEntity : class
        {
            var entry = _context.Remove(entity);
            return entry.Entity;
        }

        public void DeleteRange<TEntity>(IEnumerable<TEntity> entitys) where TEntity : class
        {
            _context.RemoveRange(entitys);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }

        public IDbConnection GetConnection()
        {
            return _context.Database.GetDbConnection();
        }

        public void Detach<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
