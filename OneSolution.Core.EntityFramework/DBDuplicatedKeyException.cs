namespace OneSolution.Core.EntityFramework
{
    public class DBDuplicatedKeyException: UserException
    {
        public DBDuplicatedKeyException(string message) : base(message) { }
    }
}
