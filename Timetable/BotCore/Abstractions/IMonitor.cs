using Timetable.Helpers;
using Timetable.Models;
using VkNet.Abstractions;

namespace Timetable.BotCore.Abstractions
{
    /// <summary>
    /// Экземпляр монитора для запуска
    /// различных действий в определенное время
    /// </summary>
    public interface IMonitor
    {
        /// <summary>
        /// Экземпляр VkApi (Vknet) для отправки сообщений
        /// </summary>
        IVkApi _vkApi { get; set; }

        /// <summary>
        /// Таймер для проверки текущего времени
        /// </summary>
        Timer timer { get; set; }

        /// <summary>
        /// Время начала пар, для того чтобы каждый раз не делать запрос к бд
        /// </summary>
        IEnumerable<TimeSpan> Intervals { get; set; }

        /// <summary>
        /// Начать следить за временем
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Обновление расписания
        /// </summary>
        /// <param name="obj"></param>
        void UpdateTimetable();

        /// <summary>
        /// Отправка уведомления о начале пар
        /// </summary>
        /// <param name="group"></param>
        /// <param name="lessons"></param>
        Task SendNotifications(IEnumerable<string> codes);

        /// <summary>
        /// Пакует сообщения пользователей в код VKScript
        /// для дальнейшей рассылки пользователям.
        /// Таким образом мы обходим ограничения АПИ.
        /// В 1 вызове кода VKScript не более 25 вызовов
        /// Вызывать Execute можно 3 раза в секунду
        /// Т.е мы сможем рассылать сообщение 75 людям в секунду, вместо 3
        /// </summary>
        /// <param name="userMessages">
        /// Список из кодов VKScript для метода execute
        /// </param>
        IEnumerable<string> PackToCodes(Dictionary<string, List<long>> userMessages);

        /// <summary>
        /// Callback таймера
        /// </summary>
        /// <param name="obj"></param>
        void CheckTime(object obj);
    }
}
