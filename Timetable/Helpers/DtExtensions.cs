using System;
using Timetable.Models;

namespace Timetable.Helpers
{
    /// <summary>
    /// Расширения для работы бота
    /// </summary>
    public static class DtExtensions
    {
        /// <summary>
        /// Количество дней в рабочей недели (пн-сб)
        /// </summary>
        private const int dayInWeek = 6;

        /// <summary>
        /// Преобразование к времени ЕКБ (+5)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>Текущее время в часовом поясе +5</returns>
        public static DateTime LocalTimeNow()
        {
            return DateTime.UtcNow.AddHours(5);
        }

        /// <summary>
        /// Сменить время (Timespan) класса DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static DateTime ChangeTime(this DateTime dateTime, TimeSpan timeSpan)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds, dateTime.Kind);
        }

        /// <summary>
        /// Сравнение 2 экземпляров DateTime, не учитываются секунды
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool DateEquals(this DateTime currentTime, DateTime dateTime)
        {
            return currentTime.Date == dateTime.Date &&
                   currentTime.Hour == dateTime.Hour &&
                   currentTime.Minute == dateTime.Minute;
        }

        /// <summary>
        /// Сравнение 2 экземплятов TimeSpan, не учитываются секунды
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool TimeEquals(this TimeSpan currentTime, TimeSpan time)
        {
            return currentTime.Hours == time.Hours &&
                   currentTime.Minutes == time.Minutes;
        }

        /// <summary>
        /// Возвращает список всех рабочих дней недели
        /// для текущей даты
        /// </summary>
        /// <param name="dateTime">
        /// Дата, дни недели которой необходимо определить
        /// </param>
        /// <returns>
        /// Диапазон из 6 дней (пн - сб) в которой
        /// находится текущая дата
        /// </returns>
        public static IEnumerable<DateTime> GetWeekDays(this DateTime dateTime)
        {
            /*
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6*/

            int daysToFirst = 1 - (int)dateTime.DayOfWeek;
            DateTime firstDay = dateTime.AddDays(daysToFirst);
            for (int i = 0; i < dayInWeek; i++)
            {
                yield return firstDay.AddDays(i);
            }
        }
    }
}
