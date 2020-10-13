using System.Linq;

namespace OneSolution.Core.EntityFramework
{
    public interface ILinqQuery<TEntity> : ILinqQuery<TEntity, TEntity>
    {
    }

    public interface ILinqQuery<out TResultEntity, TEntity>
    {
        IQueryable<TEntity> DataSource { get; set; }

        IQueryable<TResultEntity> LinqQuery { get; }
    }
}
