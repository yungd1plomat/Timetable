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
        void SendNotifications(IEnumerable<Lesson> lessons);

        /// <summary>
        /// Callback таймера
        /// </summary>
        /// <param name="obj"></param>
        void CheckTime(object obj);
    }
}
