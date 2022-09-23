namespace Timetable.Models.Bot
{
    public class Interval
    {
        public IList<Lesson> Lessons { get; set; }

        public TimeSpan Time { get; set; }

        public Interval(TimeSpan time, IList<Lesson> lessons)
        {
            Time = time;
            Lessons = lessons;
        }
    }
}
