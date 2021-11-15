using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XMLSugar
{
    public static class Extensions
    {
        public static T FirstOrNull<T>(this IEnumerable<T> values, Func<T, bool> predicate = null) where T : class
        {
            if (predicate == null) return values.DefaultIfEmpty(null).FirstOrDefault();
            return values.DefaultIfEmpty(null).FirstOrDefault(predicate);
        }

        public static T LastOrNull<T>(this IEnumerable<T> values, Func<T, bool> predicate = null) where T : class
        {
            if (predicate == null) return values.DefaultIfEmpty(null).LastOrDefault();
            return values.DefaultIfEmpty(null).LastOrDefault(predicate);
        }

        public static void CheckInterface<T>(this Type type)
        {
            if (!typeof(T).IsAssignableFrom(type))
                throw new Exception($"Expected type {typeof(T).Name} but {type.Name} was given.");
        }

        public static T CreateInstance<T>(this Type type)
        {
            type.CheckInterface<T>();
            return (T)Activator.CreateInstance(type);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null) { throw new ArgumentNullException(nameof(dictionary)); } // using C# 6
            if (key == null) { throw new ArgumentNullException(nameof(key)); } //  using C# 6

            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            int count = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    ++count;
                    list.RemoveAt(i);
                }
            }
            return count;
        }

        public static int? ToNullableInt(this string s, System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Any)
        {
            int i;
            if (int.TryParse(s, style, null, out i)) return i;
            return null;
        }

        public static bool StartsWithNumber(this string s) => Regex.IsMatch(s, @"^\d+");

        public static List<T> RemoveDuplicates<T>(this List<T> values, Func<T, T, bool> predicate = null) where T : class
        {
            var ret = new List<T>();

            var indexesToRemove = new List<int>();

            for (var aItemIdx = 0; aItemIdx < values.Count; aItemIdx++)
            {
                var aItem = values[aItemIdx];

                var foundIndexes = new List<int>();

                for (var bItemIdx = 0; bItemIdx < values.Count; bItemIdx++)
                {
                    var bItem = values[bItemIdx];
                    var found = (predicate == null) ? (aItem == bItem) : predicate(aItem, bItem);
                    if (found) foundIndexes.Add(bItemIdx);
                }

                if (foundIndexes.Count > 1)
                {
                    //var hasDuplicates = ret.Where(t => (predicate == null) ? (aItem == t) : predicate(aItem, t));
                    //if (hasDuplicates.Count() == 0)
                    if (!ret.Contains(aItem))
                        ret.Add(aItem);
                    indexesToRemove.AddRange(foundIndexes);
                }
            }

            for (var aItemIdx = values.Count; aItemIdx >= 0; aItemIdx--)
                if (indexesToRemove.Contains(aItemIdx)) values.RemoveAt(aItemIdx);

            return ret;
        }
    }
}
