using Timetable.Models.Payment;

namespace Timetable.BotCore.Abstractions
{
    /// <summary>
    /// Статус оплаты счёта
    /// </summary>
    public enum PaymentStatus
    {
        WAITING,
        PAID,
        REJECTED,
        EXPIRED,
        NONE
    }

    /// <summary>
    /// Экземпляр для работы с qiwi p2p api
    /// </summary>
    public interface IQiwiPayment
    {
        /// <summary>
        /// Выставление счета через форму
        /// https://developer.qiwi.com/ru/p2p-payments/?shell#http
        /// </summary>
        /// <returns>
        /// Ссылку на оплату счета
        /// </returns>
        Task<ResponseData> CreatePayment();

        /// <summary>
        /// Проверка оплаты счета
        /// </summary>
        /// <param name="billId"></param>
        /// <returns>
        /// enum типа Status
        /// </returns>
        Task<PaymentStatus> CheckPayment(string billId);

        /// <summary>
        /// Отмена неоплаченого счета
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        Task RejectPayment(string billId);

        /// <summary>
        /// Получить баланс qiwi кошелька
        /// </summary>
        /// <returns>
        /// Баланс
        /// </returns>
        Task<double> GetBalance();
    }
}
