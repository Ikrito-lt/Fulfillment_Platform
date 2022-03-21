using System;
using System.Collections.Generic;
using System.Linq;

namespace Ikrito_Fulfillment_Platform.Utils {

    public static class StringExt {

        /// <summary>
        /// used to trunkate string to certain char lenght
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength) {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// for getting beggining of string until certain char (-)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="stopAt"></param>
        /// <returns></returns>
        public static string GetUntilOrEmpty(this string text, string stopAt = "-") {
            if (!String.IsNullOrWhiteSpace(text)) {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0) {
                    return text.Substring(0, charLocation);
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// for getting dateTime in str form form unix timestamp 
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static string UnixTimeToSrt(this string unixTime)
        {

            DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(int.Parse(unixTime)).ToLocalTime();
            return dateTime.ToString("MM/dd/yyyy HH:mm");
        }

        /// <summary>
        /// for getting dateTime from unix timestamp 
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTime(this string unixTime)
        {

            DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(double.Parse(unixTime)).ToLocalTime();
            return dateTime;
        }
    }

    public static class DictionaryExt{

        /// <summary>
        /// for getting first key by value from dictionary
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static K GetFirstKeyByValue<K, V>(this Dictionary<K, V> dict, V val) {
            return dict.FirstOrDefault(entry =>
                EqualityComparer<V>.Default.Equals(entry.Value, val)).Key;
        }

        /// <summary>
        /// used to add dictionary to dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="source"></param>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    // handle duplicate key issue here
                    throw new Exception("Dublicate key in Dictionary.AddRange()");
                }
            }
        }
    }

    public static class ListExt
    {
        /// <summary>
        ///  method allows to get index in foreach loop of an list
        ///  Usage: foreach (var (item, index) in myValues.LoopIndex())
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<(T item, int index)> LoopIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));
    }
}
