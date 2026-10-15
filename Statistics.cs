using System;
using System.Collections.Generic;
using System.Linq;

namespace IpLab1
{
    public static class Statistics
    {
        /// <summary>
        /// Вычисляет среднее арифметическое списка чисел.
        /// </summary>
        public static double Average(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Список не может быть пустым");
            return list.Sum() / list.Count;
        }

        /// <summary>
        /// Вычисляет медиану списка чисел.
        /// </summary>
        public static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
                throw new ArgumentException("Список не может быть пустым");

            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
        }
    }
}