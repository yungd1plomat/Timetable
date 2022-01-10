namespace Timetable.Models.Payment
{
    public class ResponseData
    {
        /// <summary>
        /// Ваш идентификатор в системе p2p.qiwi
        /// </summary>
        public string siteId { get; set; }

        /// <summary>
        /// Уникальный идентификатор счета в вашей системе, указанный при выставлении
        /// </summary>
        public string billId { get; set; }

        /// <summary>
        /// Данные о сумме счета
        /// </summary>
        public Amount amount { get; set; }

        /// <summary>
        /// Данные о статусе счета
        /// </summary>
        public Status status { get; set; }

        /// <summary>
        /// Идентификаторы пользователя
        /// </summary>
        public Customer customer { get; set; }

        /// <summary>
        /// Объект строковых дополнительных параметров, переданных вами
        /// </summary>
        public CustomFields customFields { get; set; }

        /// <summary>
        /// Комментарий к счету
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// Системная дата создания счета
        /// </summary>
        public DateTime creationDateTime { get; set; }

        /// <summary>
        /// Срок действия созданной формы для перевода
        /// </summary>
        public DateTime expirationDateTime { get; set; }

        /// <summary>
        /// Ссылка для переадресации пользователя на созданную форму
        /// </summary>
        public string payUrl { get; set; }
    }
}
