using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timetable.Models
{
    [Table("GroupList")]
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Id группы на asu.bspu.ru
        /// </summary>
        public long GroupIdentifyId { get; set; }

        /// <summary>
        /// Имя группы
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Факультет
        /// </summary>
        public string Faculty { get; set; }


        /// <summary>
        /// Курс
        /// </summary>
        public int Course { get; set; }


        /// <summary>
        /// Хеш последнего расписания на asu.bspu.ru (нужно чтобы не парсить каждый раз)
        /// </summary>
        public string? Hash { get; set; }
    }
}
