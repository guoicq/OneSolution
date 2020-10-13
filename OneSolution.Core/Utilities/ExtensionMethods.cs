using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Xml.Serialization;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace OneSolution.Core.Utilities
{
    public class Localizations : Attribute
    {
        public Dictionary<int, string> Resources;
        public Localizations(string param)
        {
            Resources = JsonSerializer.Deserialize<List<KeyValuePair<int, string>>>(param).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public struct Interval
    {
        public int Start;
        public int Length;
    }

    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Get the associated localization for the given enum value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lcid"></param>
        /// <returns></returns>
        public static string GetLocalization(this Enum source, int lcid = 4105)
        {
            var youreMyType = source.GetType();
            var member = youreMyType.GetMember(source.ToString()).FirstOrDefault();
            var attribute = member?.GetCustomAttributes(typeof(Localizations), false).FirstOrDefault() as Localizations;

            if (attribute != null && attribute.Resources.TryGetValue(lcid, out var result))
                return result;

            return source.ToString();
        }

        public static IEnumerable<int> Enumerate(this Interval source)
        {
            return Enumerable.Range(source.Start, source.Length);
        }

        public static IEnumerable<T> NullSafe<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return Enumerable.Empty<T>();
            else
                return source;
        }

        public static T SafeElementAt<T>(this IList<T> source, int index) where T : new()
        {
            if (index < source.Count)
                return source[index];

            return new T();
        }

        public static T SafeElementAt<T>(this T[] source, int index) where T : new()
        {
            if (index < source.Length)
                return source[index];

            return new T();
        }

        public static IEnumerable<T> SafeTake<T>(this IList<T> source, int count) where T : new()
        {
            // Ensure that the requested number of elements are always returned
            for (var i = 0; i < count; i++)
                yield return source.SafeElementAt(i);
        }

        public static T[] NullIfEmpty<T>(this T[] source)
        {
            if (source == null)
                return null;

            if (source.Length > 0)
                return source;
            else
                return null;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item)
        {
            return source.Where(a => !object.Equals(a, item));
        }

        public static Tv TryGet<Tk, Tv>(this IDictionary<Tk, Tv> dict, Tk key)
        {
            return TryGet<Tk, Tv>(dict, key, null);
        }

        public static Tv TryGet<Tk, Tv>(this IDictionary<Tk, Tv> dict, Tk key,
            Func<Tv> createNew)
        {
            Tv value = default(Tv);
            if (dict == null)
                return value;

            lock (dict)
            {
                if (!dict.TryGetValue(key, out value) && createNew != null)
                {
                    value = createNew();
                    dict.Add(key, value);
                }
            }

            return value;
        }

        public static void AddRange<Tk, Tv>(this IDictionary<Tk, Tv> dict, IEnumerable<KeyValuePair<Tk, Tv>> items)
        {
            foreach (var item in items)
                dict.Add(item);
        }

        public static IEnumerable<T> Range<T>(this List<T> source, Interval interval)
        {
            return Range(source, interval.Start, interval.Length);
        }

        public static IEnumerable<T> Range<T>(this List<T> source, int start, int length)
        {
            for (int i = 0; i < length; i++)
                yield return source[start + i];
        }

        public static bool In<T>(this T value, params T[] set)
        {
            return set.Contains(value);
        }

        public static T NullIf<T>(this T value, Func<T, bool> predicate)
            where T : class
        {
            return predicate(value) ? null : value;
        }

        public static bool In<T>(this T value, IEnumerable<T> set)
        {
            return set.Contains(value);
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            int index = 0;
            foreach (var si in source)
            {
                if (object.Equals(item, si))
                    return index;
                index++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (var si in source)
            {
                if (predicate(si))
                    return index;
                index++;
            }
            return -1;
        }

        public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return IndexOf(source, predicate) >= 0;
        }

        public static bool Between<T>(this T value, T from, T to)
            where T : IComparable<T>
        {
            return value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
        }

        public class EnumeratorWithStatus<T>
        {
            public bool hasMore = true;
            public IEnumerator<T> enumerator;
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int chunkSize)
        {
            var enumeratorWithStatus = new EnumeratorWithStatus<T> {
                enumerator = source.GetEnumerator()
            };

            while (enumeratorWithStatus.hasMore)
                yield return getChunk(enumeratorWithStatus, chunkSize).ToList();
        }

        private static IEnumerable<T> getChunk<T>(EnumeratorWithStatus<T> enumeratorWithStatus, int chunkSize)
        {
            while (chunkSize > 0)
            {
                if (enumeratorWithStatus.enumerator.MoveNext())
                {
                    chunkSize--;
                    yield return enumeratorWithStatus.enumerator.Current;
                }
                else
                {
                    enumeratorWithStatus.hasMore = false;
                    yield break;
                }
            }
        }

        public static string ExpandVariables(this string value, Func<string, string> replace,
            string openBrace = "{", string closeBrace = "}", bool replaceNullValues = true)
        {
            if (value != null)
            {
                int start = 0;
                for (start = value.IndexOf(openBrace, start); start >= 0; start = value.IndexOf(openBrace, start))
                {
                    int end = value.IndexOf(closeBrace, start);

                    if (end >= 0 && end > start)
                    {
                        string key = value.Substring(start + openBrace.Length, end - start - openBrace.Length);
                        var val = replace(key);
                        if (replaceNullValues || val != null)
                            value = value.Remove(start, end - start + closeBrace.Length);
                        if (val != null)
                        {
                            value = value.Insert(start, val);
                            start += val.Length;
                        }
                        else if (!replaceNullValues)
                            start = end + closeBrace.Length;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Finds a most common element in a sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source sequence</param>
        /// <returns></returns>
        public static T MostCommon<T>(this IEnumerable<T> source)
        {
            return source
                .GroupBy(a => a)
                .OrderByDescending(a => a.Count())
                .Select(a => a.Key)
                .First();
        }

        public static Tv MostCommon<T, Tv>(this IEnumerable<T> source, Func<T, Tv> selector)
        {
            return source
                .GroupBy(a => selector(a))
                .OrderByDescending(a => a.Count())
                .Select(a => a.Key)
                .First();
        }

        public static Tv MostCommon<T, Tv, Tsort>(this IEnumerable<T> source, Func<T, Tv> selector, Func<Tv, Tsort> sortSelector)
        {
            return source
                .GroupBy(a => selector(a))
                .OrderByDescending(a => a.Count())
                .ThenBy(a => sortSelector(a.Key))
                .Select(a => a.Key)
                .First();
        }

        public static int BitwiseOr<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            int result = 0;
            foreach (var item in source)
                result |= selector(item);
            return result;
        }

        public static int BitwiseOr(this IEnumerable<int> source)
        {
            int result = 0;
            foreach (var item in source)
                result |= item;
            return result;
        }

        public static uint BitwiseOr(this IEnumerable<uint> source)
        {
            uint result = 0;
            foreach (var item in source)
                result |= item;
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        public static void ForEach<T, d>(this IEnumerable<T> source, Func<T, d> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static IEnumerable<T> Enumerate<T>(this IEnumerator<T> source)
        {
            using (source)
                while (source.MoveNext())
                    yield return source.Current;
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, ParallelOptions parallelOptions, Func<T, Task> body)
        {
            var partitionTasks = Partitioner.Create(source)
                .GetPartitions(parallelOptions == null ? SysUtils.CPUThreads : parallelOptions.MaxDegreeOfParallelism)
                .Select(partition => {
                    var tasks = partition.Enumerate().Select(task => body(task)).ToArray();
                    if (tasks.Length > 0)
                        return Task.Factory.ContinueWhenAll(tasks, t => t);
                    else
                        return null;
                })
                .Where(t => t != null)
                .ToArray();

            return Task.Factory.ContinueWhenAll(partitionTasks, t => t);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            return ForEachAsync(source, null, body);
        }

        /// <summary>
        /// Similar to ToList(), produces a HashSet
        /// </summary>
        /// <typeparam name="T">the type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source"><c>IEnumerable&lt;T&gt;</c> of <typeparamref name="T"/></param>
        /// <param name="comparer">Comparer to pass to <c>HashSet</c> constructor</param>
        /// <returns><c>HashSet&lt;T&gt;</c></returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null)
                return new HashSet<T>(source);
            else
                return new HashSet<T>(source, comparer);
        }

        public static HashSet<T> ToHashSet<Tsrc, T>(this IEnumerable<Tsrc> source, Func<Tsrc, T> selector)
        {
            return new HashSet<T>(source.Select(selector));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, Func<T, int> getHashCode, Func<T, T, bool> equals)
        {
            return new HashSet<T>(source, new EqualityComparer<T>(getHashCode, equals));
        }

        public static Stack<T> ToStack<T>(this IEnumerable<T> source)
        {
            return new Stack<T>(source);
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            return new Queue<T>(source);
        }

        public static IEnumerable<TResult> IntersectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            HashSet<TResult> resultSet = null;

            foreach (var set in source)
            {
                if (resultSet == null)
                {
                    resultSet = new HashSet<TResult>();
                    resultSet.AddRange(selector(set));
                }
                else
                    resultSet.IntersectWith(selector(set));
            }

            return resultSet ?? Enumerable.Empty<TResult>();
        }

        public static Tsource AddRange<T, Tsource>(this ISet<T> set, Tsource source)
            where Tsource : IEnumerable<T>
        {
            foreach (var item in source)
                set.Add(item);

            return source;
        }

        public static Dictionary<Tkey, Tvalue> ToDictionary<T, Tkey, Tvalue>(
            this IEnumerable<T> source, Func<T, Tkey> keySelector, Func<T, Tvalue> valueSelector,
            Func<Tkey, Tvalue, bool> duplicateKeyNotify,
            IEqualityComparer<Tkey> comparer = null)
        {
            var dictionary = new Dictionary<Tkey, Tvalue>(comparer);
            foreach (var item in source)
            {
                var key = keySelector(item);
                var value = valueSelector(item);

                if (dictionary.ContainsKey(key))
                {
                    if (!duplicateKeyNotify(key, value))
                        return null;
                }
                else
                    dictionary.Add(key, value);
            }
            return dictionary;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            yield return item;
            foreach (var si in source)
                yield return si;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
                yield return item;
            foreach (var si in source)
                yield return si;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            foreach (var si in source)
                yield return si;
            yield return item;
        }

        public static TList AppendToList<T, TList>(this TList source, T item)
            where TList : IList<T>
        {
            source.Add(item);
            return source;
        }

        public static string StringJoin(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }

        public static string StringJoin<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join<T>(separator, source);
        }

        public static string Truncate(this string str, int maxLength)
        {
            if (str == null || str.Length <= maxLength)
                return str;
            else
                return str.Substring(0, maxLength);
        }

        public static double? AverageNullable<T>(this IEnumerable<T> source, Func<T, double?> selector)
        {
            return AverageNullable(source.Select(selector));
        }

        public static double? AverageNullable(this IEnumerable<double?> source)
        {
            double sum = 0;
            int len = 0;

            foreach (var element in source)
            {
                if (element.HasValue)
                {
                    sum += element.Value;
                    len++;
                }
                else
                    return null;
            }
            return sum / len;
        }

        public static double AverageWeighted<T>(this IEnumerable<T> source, Func<T, double> valueSelector,
            Func<T, double> weightSelector, double totalWeight)
        {
            if (totalWeight == 0)
                return 0;
            else
                return source.Sum(element => valueSelector(element) * weightSelector(element)) / totalWeight;
        }

        public static double AverageWeighted<T>(this IEnumerable<T> source, Func<T, double> valueSelector,
            Func<T, double> weightSelector)
        {
            double totalWeight = 0;
            double sum = 0;

            foreach (var element in source)
            {
                sum += valueSelector(element) * weightSelector(element);
                totalWeight += weightSelector(element);
            }
            if (totalWeight == 0)
                return 0;
            else
                return sum / totalWeight;
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(e => e != null);
        }

        public static int GetSequenceHashCode<T>(this IEnumerable<T> source)
        {
            return source.NotNull().Select(x => x.GetHashCode()).Aggregate((x, y) => x ^ y);
        }

        public static T DefaultIf<T>(this T source, Func<T, bool> predicate, T defaultValue = default(T))
        {
            if (predicate(source))
                return defaultValue;
            else
                return source;
        }

        private static Dictionary<Type, PropertyInfo[]> propertyCache = new Dictionary<Type, PropertyInfo[]>();
        private static Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        public static IEnumerable<(string Key, Tvalue Value)> GetValues<T, Tvalue>(this T source, Func<object, Tvalue> valueSubstitution, bool cacheFields = false)
        {
            PropertyInfo[] properties;
            if (cacheFields)
                lock (propertyCache)
                    properties = propertyCache.TryGet(typeof(T), () => source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public));
            else
                properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
                yield return (property.Name, valueSubstitution(property.GetValue(source, null)));

            FieldInfo[] fields;
            if (cacheFields)
                lock (fieldCache)
                    fields = fieldCache.TryGet(typeof(T), () => source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public));
            else
                fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
                yield return (field.Name, valueSubstitution(field.GetValue(source)));
        }

        public static Dictionary<string, object> AsDictionary<T>(this T source, bool cacheFields = false)
        {
            return AsDictionary(source, v => v, cacheFields);
        }

        public static Dictionary<string, Tvalue> AsDictionary<T, Tvalue>(this T source, Func<object, Tvalue> valueSubstitution = null, bool cacheFields = false)
        {
            return GetValues(source, valueSubstitution, cacheFields)
                .ToDictionary(a => a.Key, a => a.Value);
        }

        public static Dictionary<string, object> AsDictionary<T>(this IDataReader source)
        {
            var result = new Dictionary<string, object>();
            for (int i = 0; i < source.FieldCount; i++)
                result[source.GetName(i)] = source.GetValue(i);
            return result;
        }

        public static Dictionary<TKey, TValue> GetMerged<TKey, TValue>(
            this IEnumerable<IDictionary<TKey, TValue>> dictionariesToMerge,
            Func<TValue, TValue, TValue> valueConflictResolver = null)
        {
            var outputDictionary = new Dictionary<TKey, TValue>();

            foreach (var currentDictionary in dictionariesToMerge)
            {
                foreach (var key in currentDictionary.Keys)
                {
                    if (outputDictionary.ContainsKey(key))
                    {
                        if (valueConflictResolver == null)
                        {
                            throw new Exception($"duplicate key {key} encountered and no valueConflictResolver supplied. Can't merge values.");
                        }

                        outputDictionary[key] = valueConflictResolver(outputDictionary[key], currentDictionary[key]);
                    }
                    else
                    {
                        outputDictionary[key] = currentDictionary[key];
                    }
                }
            }

            return outputDictionary;
        }

        public static Dictionary<TKey, TValue> GetMerged<TKey, TValue>(
            this IDictionary<TKey, TValue> dict1,
            IDictionary<TKey, TValue> dict2,
            Func<TValue, TValue, TValue> valueConflictResolver = null)
        {
            return new[] { dict1, dict2 }.GetMerged(valueConflictResolver);
        }


        public static IEnumerable<Tuple<T1, T2>> Splice<T1, T2>(this IEnumerable<T1> source1, IEnumerable<T2> source2, bool truncateUnequalLengthSources = false)
        {
            using (var enumerator1 = source1.GetEnumerator())
            using (var enumerator2 = source2.GetEnumerator())
            {
                while (true)
                {
                    var hasNext1 = enumerator1.MoveNext();
                    var hasNext2 = enumerator2.MoveNext();

                    if (hasNext1 != hasNext2 && !truncateUnequalLengthSources)
                    {
                        throw new ArgumentException("source enumerables were of unequal length");
                    }

                    if (!hasNext1 || !hasNext2) yield break;

                    yield return new Tuple<T1, T2>(enumerator1.Current, enumerator2.Current);
                }
            }
        }

        public static IEnumerable<IEnumerable<Tv>> ZipMerge<Tk, Tv>(this IEnumerable<IEnumerable<Tk>> sources, Func<Tk, Tv> selector)
        {
            var enumerators = sources.Select(s => s.GetEnumerator()).ToList();
            try
            {
                while (true)
                {
                    bool hasNext = enumerators.All(e => e.MoveNext());
                    if (!hasNext)
                        yield break;

                    yield return enumerators.Select(e => selector(e.Current));
                }
            }
            finally
            {
                enumerators.ForEach(e => e.Dispose());
            }
        }

        public class DiffResult<T>
        {
            public IEnumerable<T> Common;
            public IEnumerable<T> OnlyInA;
            public IEnumerable<T> OnlyInB;
        }

        public static DiffResult<T> Diff<T>(this IEnumerable<T> sourceA, IEnumerable<T> sourceB,
            Func<T, int> getHashCode, Func<T, T, bool> equals)
        {
            var uniqueA = sourceA.ToHashSet(getHashCode, equals);
            var uniqueB = sourceB.ToHashSet(getHashCode, equals);

            return new DiffResult<T> {
                Common = uniqueA.Where(item => uniqueB.Contains(item)),
                OnlyInA = uniqueA.Where(item => !uniqueB.Contains(item)),
                OnlyInB = uniqueB.Where(item => !uniqueA.Contains(item))
            };
        }

        public static DiffResult<T> Diff<T>(this IEnumerable<T> sourceA, IEnumerable<T> sourceB,
            IEqualityComparer<T> comparer = null)
        {
            var uniqueA = sourceA.ToHashSet(comparer);
            var uniqueB = sourceB.ToHashSet(comparer);

            return new DiffResult<T> {
                Common = uniqueA.Where(item => uniqueB.Contains(item)),
                OnlyInA = uniqueA.Where(item => !uniqueB.Contains(item)),
                OnlyInB = uniqueB.Where(item => !uniqueA.Contains(item))
            };
        }

        public static T ValueDefault<T>(this T? source)
            where T : struct
        {
            if (source.HasValue)
                return source.Value;
            else
                return default(T);
        }

        public static TOut Invoke<TIn, TOut>(this TIn source, Func<TIn, TOut> func)
        {
            return func(source);
        }

        public static IEnumerable<(T Value, int IterNum)> Enumerate<T>(this IEnumerable<T> source)
        {
            return source.Select((v, i) => new ValueTuple<T, int>(v, i));
        }

        /// <summary>
        /// groups together adjacent elements after sorting
        /// It seems that GroupSequence() can do something similar or identical but it freezes when I call
        /// ToList() on it so a TODO is to figure out what is wrong with GroupSequence() and whether this function provides only a subset of the functionality of GroupSequence()
        /// </summary>
        public static IEnumerable<IList<T>> SortAndGroup<T>(this IEnumerable<T> source, Func<T, T, bool> adjacentElementMatcherFunc)
            where T : IComparable<T>
        {
            var enumerator = source.OrderBy(v => v).GetEnumerator();

            if (!enumerator.MoveNext())
                yield break;

            var hasCurrent = true;
            while (hasCurrent)
            {
                var outputList = new List<T>
                {
                    enumerator.Current
                };

                hasCurrent = enumerator.MoveNext();

                while (hasCurrent && adjacentElementMatcherFunc(outputList[outputList.Count - 1], enumerator.Current))
                {
                    outputList.Add(enumerator.Current);
                    hasCurrent = enumerator.MoveNext();
                }
                yield return outputList;
            }
        }

        public static string SerializeXml(object toSerialize, XmlSerializerNamespaces namespaces = null)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize, namespaces);
                return textWriter.ToString();
            }
        }

        public static T DeserializeXml<T>(this string objectData)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (TextReader reader = new StringReader(objectData))
                return (T)serializer.Deserialize(reader);
        }

        public static bool ContainsAtLeastNCharacters(this string oriString, int nCharacters)
        {
            return oriString != null && oriString.Length >= nCharacters;
        }

        public static bool ContainsAtLeastOneLowerCaseCharacter(this string oriString)
        {
            return oriString != null && oriString.Any(char.IsLower);
        }

        public static bool ContainsAtLeastOneUpperCaseCharacter(this string oriString)
        {
            return oriString != null && oriString.Any(char.IsUpper);
        }

        public static bool ContainsAtLeastOneNumber(this string oriString)
        {
            return oriString != null && oriString.Any(char.IsDigit);
        }

        public static bool IsProvince(this string oriString)
        {
            string[] provinces = { "AB", "BC", "MB", "NB", "NL", "NT", "NS", "NU", "ON", "PE", "QC", "SK", "YT" };
            return oriString != null && Array.IndexOf(provinces, oriString) > -1;
        }

        public static void Append<Tk, Tv>(this Dictionary<Tk, Tv> obj, Dictionary<Tk, Tv> with)
        {
            if (with == null)
                return;

            foreach (Tk key in with.Keys.Where(a => !obj.ContainsKey(a)))
            {
                obj.Add(key, with[key]);
            }
        }

        public static IEnumerable<string> ReadLines(this TextReader source)
        {
            string line;
            while ((line = source.ReadLine()) != null)
                yield return line;
        }

        public static string TrimLast(this string source, char ch)
        {
            if (string.IsNullOrEmpty(source))
                return source;
            if (source[source.Length - 1] == ch)
                return source.Substring(0, source.Length - 1);
            else
                return source;
        }

        public static MemoryStream ToMemory(this Stream source, bool disposeSource = true)
        {
            var ms = new MemoryStream();
            source.CopyTo(ms);
            if (disposeSource)
                source.Dispose();
            return ms;
        }

        public static bool Remove<Tkey, Tvalue>(this ConcurrentDictionary<Tkey, Tvalue> source, Tkey key)
        {
            Tvalue value;
            return source.TryRemove(key, out value);
        }

        public static T AsEnum<T>(this string value) where T : struct
        {
            T result;
            if (Enum.TryParse<T>(value, out result))
                return result;
            else
                throw new FormatException($"{value} cannot be parsed as {typeof(T)}");
        }

        public static T SingleOrNothing<T>(this IEnumerable<T> source)
        {
            T first = default(T);
            bool found = false;
            foreach (var item in source)
            {
                if (!found)
                {
                    found = true;
                    first = item;
                }
                else if (object.Equals(first, item))
                {
                    return default(T);
                }
            }
            return first;
        }

        // Below NextLong implementations pilfered from http://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way
        public static long NextLong(this Random random, long min, long max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException(nameof(max), "max must be > min!");

            ulong uRange = (ulong)(max - min);
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        public static long NextLong(this Random random, long max)
        {
            return random.NextLong(0, max);
        }

        public static long NextLong(this Random random)
        {
            return random.NextLong(long.MinValue, long.MaxValue);
        }

        public static double Round(this double number, int decimals)
        {
            return Math.Round(number, decimals);
        }

        public static double? Round(this double? number, int decimals)
        {
            if (!number.HasValue)
                return number;
            else
                return Math.Round(number.Value, decimals);
        }

        public static bool IsNaN(this double? number)
        {
            return number.HasValue && double.IsNaN(number.Value);
        }

        public static bool IsNaNorInfinity(this double? number)
        {
            return number.HasValue && (double.IsNaN(number.Value) || double.IsInfinity(number.Value));
        }
        public static bool IsNullorNaNorInfinity(this double? number)
        {
            return !number.HasValue || double.IsNaN(number.Value) || double.IsInfinity(number.Value);
        }

        public static double IfNaN(this double number, double replace)
        {
            return double.IsNaN(number) ? replace : number;
        }

        public static double IfNaNorInfinity(this double number, double replace)
        {
            return double.IsNaN(number) || double.IsInfinity(number) ? replace : number;
        }

        public static byte[] GetBytes(this RandomNumberGenerator rng, int length)
        {
            var ar = new byte[length];
            rng.GetBytes(ar);
            return ar;
        }

        public static int GetNumber(this RandomNumberGenerator rng, int fromInclusive, int toExclusive)
        {
            if (toExclusive < fromInclusive)
            {
                var tmp = toExclusive;
                toExclusive = fromInclusive;
                fromInclusive = tmp;
            }

            var u = BitConverter.ToUInt32(rng.GetBytes(sizeof(uint)), 0);
            return (int)(fromInclusive + (u % (toExclusive - fromInclusive)));
        }

        public static int TransformBlock(this HashAlgorithm hash, byte[] block)
        {
            return hash.TransformBlock(block, 0, block.Length, block, 0);
        }

        public static string Dump(this object obj)
        {
            if (obj == null)
                return "null";

            return $"{{ {obj.GetValues(v => v, true).Select(a => $"{a.Key}: {a.Value}").StringJoin(", ")} }}";
        }

        public static IEnumerable<string> Dump(this IEnumerable<object> source)
        {
            return source.NullSafe().Select(Dump);
        }

        public static void Dump(this IEnumerable<object> source, Stream stream)
        {
            using (stream)
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 0))
                foreach (var str in source.Dump())
                    sw.WriteLine(str);
        }

        public static string SanitizeFileName(this string source)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                source = source.Replace(c, '_'); // use underscore as the replacement character--just like windows/chrome

            return source;
        }

        private static string accentedChars = "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝßàáâãäåçèéêëìíîïðñòóôõöøùúûüýÿŸ";
        private static string standardChars = "AAAAAACEEEEIIIIDNOOOOOOUUUUYSaaaaaaceeeeiiiidnoooooouuuuyyY";

        /// <summary>
        /// Warning: Works for French only<para>
        ///   For a more robust solution see stackoverflow.com</para>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveAccents(this string source)
        {
            return source;  // TODO
        }

        public static void SaveAs(this string source, string fullPath)
        {
            using (var writer = new StreamWriter(fullPath))
            {
                writer.Write(source);
            }
        }

        private static string OrderJsonSerializedObjectByFieldName(string source)
        {
            var result = new StringBuilder();

            Dictionary<string, object> deserialized = null;

            try
            {
                deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(source);
            }
            catch
            {
                return source;
            }

            if (deserialized == null)
                return source;

            result.Append("{");

            foreach (var key in deserialized.Keys.OrderBy(a => a))
                result.Append($"{(result.Length > 1 ? "," : "")}\"{key}\":{JsonSerializer.Serialize(deserialized[key]).OrderJsonSerializedObject()}");

            return result.Append("}").ToString();
        }

        public static string OrderJsonSerializedObject(this string source)
        {
            if (source.StartsWith("["))
            {
                var result = new StringBuilder();
                var deserialized = JsonSerializer.Deserialize<List<object>>(source);

                // before ordering the list, order the fields within each element...
                for (var index = 0; index < deserialized.Count; index++)
                    deserialized[index] = OrderJsonSerializedObjectByFieldName(JsonSerializer.Serialize(deserialized[index]));

                result.Append("[");

                foreach (var item in deserialized.OrderBy(a => a))
                    result.Append($"{(result.Length > 1 ? "," : "")}{item}");

                return result.Append("]").ToString();
            }

            return OrderJsonSerializedObjectByFieldName(source);
        }
    }


}
