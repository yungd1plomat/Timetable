namespace Timetable.Models.Bot
{
    public class Interval
    {
        public IList<Day> Days { get; set; }

        public TimeSpan Time { get; set; }

        public Interval(TimeSpan time)
        {
            Time = time;
            Days = new List<Day>();
        }
    }
}
