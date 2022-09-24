using System.Globalization;

namespace Timetable.Models.Bot
{
    public class Day
    {
        public IList<Lesson> Lessons { get; set; }

        public string DayName { get; set; }

        public string Date { get; set; }


        public Day(DateTime dateTime, IList<Lesson> lessons)
        {
            DayName = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(dateTime.DayOfWeek);
            Date = dateTime.ToString("dd.MM.yy");
            Lessons = lessons;
        }
    }
}
