namespace Timetable.Models.Payment
{
    public class Amount
    {
        /// <summary>
        /// Валюта суммы счета. Возможные значения:
        /// RUB - рубли
        /// KZT - тенге
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Сумма, на которую выставляется счет, округленная в меньшую сторону до 2 десятичных знаков
        /// </summary>
        public decimal value { get; set; }
    }
}
