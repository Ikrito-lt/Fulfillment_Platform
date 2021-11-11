using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Utils {

    public static class ExtensionMethods {

        public static int Remap(this int value, int from1, int to1, int from2, int to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source) {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            foreach (var element in source)
                target.Add(element);
        }

        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection) {
            if (collection == null) {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection) {
                if (!source.ContainsKey(item.Key)) {
                    source.Add(item.Key, item.Value);
                } else {
                    throw new Exception($"Dublicate Key: {item.Key} val: {item.Value}");
                }
            }
        }

        //for getting dateTime in str form form unix timestamp
        public static string UnixTimeToSrt(this string unixTime) {

            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(int.Parse(unixTime)).ToLocalTime();
            return dateTime.ToString("MM/dd/yyyy HH:mm");
        }
        
        //for getting beggining of string until certain char (-)
        public static string GetUntilOrEmpty(this string text, string stopAt = "-") {
            if (!String.IsNullOrWhiteSpace(text)) {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0) {
                    return text.Substring(0, charLocation);
                }
            }
            return String.Empty;
        }

        //for getting first key by value from dictionary
        public static K FindFirstKeyByValue<K, V>(this Dictionary<K, V> dict, V val) {
            return dict.FirstOrDefault(entry =>
                EqualityComparer<V>.Default.Equals(entry.Value, val)).Key;
        }

    }
}
