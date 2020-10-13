using System;
using System.Collections.Generic;

namespace OneSolution.Core.Utilities
{

    public static class EqualityComparer
    {
        public static EqualityComparer<T> Create<T>(Func<T, int> getHashCode, Func<T, T, bool> equals)
        {
            return new EqualityComparer<T>(getHashCode, equals);
        }
    }

    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, int> getHashCode;
        private Func<T, T, bool> equals;

        public EqualityComparer(Func<T, int> getHashCode, Func<T, T, bool> equals)
        {
            this.getHashCode = getHashCode;
            this.equals = equals;
        }

        public bool Equals(T x, T y)
        {
            return equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }
}
