using System;
using System.Linq.Expressions;

namespace OneSolution.Core.EntityFramework
{
    public interface IPredicateQuery<T>
    {
        Expression<Func<T, bool>> Predicate { get; }
    }
}
