using System.Globalization;

namespace Timetable.Models.Bot
{
    public class Day
    {
        public IList<Interval> Intervals { get; set; }

        public string DayName { get; set; }

        public string Date { get; set; }


        public Day(DateTime dateTime)
        {
            DayName = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(dateTime.DayOfWeek);
            Date = dateTime.ToString("dd.MM.yy");
            Intervals = new List<Interval>();
        }

    }
}
