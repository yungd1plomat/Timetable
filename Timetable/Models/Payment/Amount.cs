namespace Timetable.Models.Payment
{
    public class Amount
    {
        /// <summary>
        /// Валюта суммы счета. Возможные значения:
        /// RUB - рубли
        /// KZT - тенге
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Сумма, на которую выставляется счет, округленная в меньшую сторону до 2 десятичных знаков
        /// </summary>
        public decimal Value { get; set; }
    }
}
