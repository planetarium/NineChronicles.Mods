using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NineChronicles.Mods.Athena.Utils
{
    public static class CsvUtil
    {
        public static void SaveToCsv<T>(string filePath, List<T> data, char delimiter = ',')
        {
            if (data == null || !data.Any())
            {
                throw new ArgumentException("Data is null or empty.");
            }

            var csv = new StringBuilder();
            var properties = typeof(T).GetProperties();

            var header = string.Join(delimiter, properties.Select(prop => prop.Name));
            csv.AppendLine(header);

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            foreach (var item in data)
            {
                var line = string.Join(delimiter, properties.Select(prop =>
                {
                    var value = prop.GetValue(item);
                    if (value is IEnumerable<object> collection && value is not string)
                    {
                        var enumerableContent = collection.Cast<object>().Select(x => x.ToString());
                        return string.Join(";", enumerableContent);
                    }
                    else if (value is IEnumerable enumerable && value is not string) // non-generic IEnumerable 처리
                    {
                        var nonGenericCollection = enumerable.Cast<object>().Select(x => x?.ToString() ?? "");
                        return string.Join(";", nonGenericCollection);
                    }
                    else
                    {
                        var output = value?.ToString() ?? "";
                        return output;
                    }
                }));
                csv.AppendLine(line);
            }

            File.WriteAllText(filePath, csv.ToString());
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }
    }
}
