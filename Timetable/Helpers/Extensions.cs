using Timetable.Models;

namespace Timetable.Helpers
{
    /// <summary>
    /// Расширения для работы бота
    /// </summary>
    public static class Extensions
    {

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
    }
}
