namespace Timetable.Models.Payment
{
    public class CreateBillRequest
    {
        /// <summary>
        /// Данные о сумме счета
        /// </summary>
        public Amount amount { get; set; }

        /// <summary>
        /// Комментарий к счету
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// Дата, до которой счет будет доступен для оплаты. 
        /// Если перевод не будет совершен до этой даты, 
        /// ему присваивается финальный статус EXPIRED и последующий перевод станет невозможен.
        /// </summary>
        public DateTime expirationDateTime { get; set; }

        /// <summary>
        /// Идентификаторы пользователя
        /// </summary>
        public Customer customer { get; set; }

        /// <summary>
        /// Дополнительные данные счета. Вы можете здесь передавать свои дополнительные поля с данными, например, SteamId
        /// </summary>
        public CustomFields customFields { get; set; }
    }
}
