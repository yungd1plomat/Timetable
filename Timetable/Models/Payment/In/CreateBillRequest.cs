namespace Timetable.Models.Payment
{
    public class CreateBillRequest
    {
        /// <summary>
        /// Данные о сумме счета
        /// </summary>
        public Amount Amount { get; set; }

        /// <summary>
        /// Комментарий к счету
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Дата, до которой счет будет доступен для оплаты. 
        /// Если перевод не будет совершен до этой даты, 
        /// ему присваивается финальный статус EXPIRED и последующий перевод станет невозможен.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// Идентификаторы пользователя
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Дополнительные данные счета. Вы можете здесь передавать свои дополнительные поля с данными, например, SteamId
        /// </summary>
        public CustomFields CustomFields { get; set; }
    }
}
