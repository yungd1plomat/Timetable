namespace Timetable.Models.Payment
{
    public class ResponseData
    {
        /// <summary>
        /// Ваш идентификатор в системе p2p.qiwi
        /// </summary>
        public string SiteId { get; set; }

        /// <summary>
        /// Уникальный идентификатор счета в вашей системе, указанный при выставлении
        /// </summary>
        public string BillId { get; set; }

        /// <summary>
        /// Данные о сумме счета
        /// </summary>
        public Amount Amount { get; set; }

        /// <summary>
        /// Данные о статусе счета
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Идентификаторы пользователя
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Объект строковых дополнительных параметров, переданных вами
        /// </summary>
        public CustomFields CustomFields { get; set; }

        /// <summary>
        /// Комментарий к счету
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Системная дата создания счета
        /// </summary>
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Срок действия созданной формы для перевода
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// Ссылка для переадресации пользователя на созданную форму
        /// </summary>
        public string PayUrl { get; set; }
    }
}
