using System;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer
{
    public class DateUtility
    {
        
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        public static double TimeSpanToValue(TimeSpan span)
        {
            return span.TotalSeconds;
        }
        public static TimeSpan ValueToTimeSpan(double value)
        {
            return TimeSpan.FromSeconds(value);
        }
        public static double DateToValue(DateTime dateTime)
        {
            return (dateTime - Epoch).TotalSeconds;
        }

        public static DateTime ValueToDate(double value)
        {
            return Epoch.AddSeconds(value);
        }
    }
}