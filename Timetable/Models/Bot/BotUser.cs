using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timetable.Models
{
    [Table("Users")]
    public class BotUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Id юезра на vk.com
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Группа пользователя
        /// </summary>
        public virtual Group? Group { get; set; }

        /// <summary>
        /// Дата истечения подписки
        /// </summary>
        public DateTime? Subscribtion { get; set; }

        /// <summary>
        /// Последний Id платежа, для защиты от повторной проверки оплаты 
        /// (в случае если сообщение с проверкой оплаты не удалится)
        /// </summary>
        public string? BillId { get; set; }

        /// <summary>
        /// Является ли пользователь админом
        /// </summary>
        public bool? admin { get; set; }

        /// <summary>
        /// Id последнего сообщения для редактирования
        /// </summary>
        public long? msgId { get; set; }
    }
}
