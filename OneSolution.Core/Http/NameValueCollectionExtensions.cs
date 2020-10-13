using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace OneSolution.Core.Http
{
    public static class NameValueCollectionExtensions
    {
        public static NameValueCollection Append(this NameValueCollection collection, string key, object value)
        {
            collection.Add(key, $"{value}");
            return collection;
        }
        public static NameValueCollection Append(this NameValueCollection collection, string key, int[] values)
        {
            if (values == null || values.Length == 0)
                return collection;

            foreach(var value in values)
                collection.Add(key, $"{value}");
            return collection;
        }

        public static string ToQueryString(this NameValueCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return null;
            var sb = new StringBuilder();
            var items = collection.AllKeys.SelectMany(collection.GetValues, (k, v) => new { Key = k, Value = v });
            foreach (var item in items)
                sb.Append($"{WebUtility.UrlEncode(item.Key)}={WebUtility.UrlEncode(item.Value)}&");
            var query = sb.ToString();
            return query.Substring(0, query.Length - 1);
        }
    }
}
