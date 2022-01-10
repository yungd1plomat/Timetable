using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timetable.Models
{
    [Table("Lessons")]
    public class Lesson
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Время начала
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Предмет
        /// </summary>
        public string Subject { get; set; }


        /// <summary>
        /// Преподаватель
        /// </summary>
        public string Teacher { get; set; }

        /// <summary>
        /// Группа
        /// </summary>
        public virtual Group Group { get; set; }

        /// <summary>
        /// Формирование короткой строки о занятии
        /// </summary>
        /// <returns>
        /// Строка с короткой информацией о текущем экземпляре
        /// </returns>
        public string ToShortString() // https://stackoverflow.com/questions/73883/string-vs-stringbuilder
        {
            return $"✅ {Subject} / {Teacher}";
        }

        /// <summary>
        /// Формирование полной строки о занятии
        /// </summary>
        /// <returns>
        /// Строка с полной информацией о текущем экземпляре
        /// </returns>
        public override string ToString()
        {
            return String.Format("✅ {0} - {1} / {2}", StartTime.TimeOfDay.ToString(@"hh\:mm"), Subject, Teacher);
        }

        /// <summary>
        /// Формирование строки вместе с датой
        /// </summary>
        /// <returns></returns>
        public string ToLongString()
        {
            return String.Format("✅ {0} - {1} / {2}", StartTime.ToString("dd.MM.yyyy HH:mm"), Subject, Teacher);
        }
    }
}
