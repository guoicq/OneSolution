using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace OneSolution.Core.EntityFramework
{
    public class DBExceptionHelper
    {
        public static void DbErrorHandler(Exception ex)
        {
            var innerEx = getInnerException(ex);

            if (IsDBDuplicatedKeyException(ex))
                // A custom exception for duplication key
                throw new DBDuplicatedKeyException($"Cannot insert duplicate row to database. {innerEx}");

            throw new DbUpdateException(ex.Message, innerEx);
        }

        public static bool IsDBDuplicatedKeyException(Exception ex)
        {
            var isDBDuplicatedKeyException = false;

            if (ex is DbUpdateException)
            {
                if (getInnerException(ex) is SqlException sqlException)
                {
                    if (sqlException.Number == 2627 // Cannot insert duplicate key: Violation of PRIMARY KEY constraint
                        || sqlException.Number == 2601) // Cannot insert duplicate key row with unique index
                    {
                        isDBDuplicatedKeyException = true;
                    }
                }
            }

            return isDBDuplicatedKeyException;
        }

        private static Exception getInnerException(Exception ex)
        {
            var innerEx = ex;
            while (innerEx.InnerException != null) innerEx = innerEx.InnerException;

            return innerEx;
        }
    }
}
