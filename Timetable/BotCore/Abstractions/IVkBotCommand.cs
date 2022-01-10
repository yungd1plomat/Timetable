using Timetable.Helpers;
using VkNet.Abstractions;

namespace Timetable.BotCore.Abstractions
{
    /// <summary>
    /// Команда для бота, которая приходит от пользователя
    /// </summary>
    public interface IVkBotCommand
    {
        /// <summary>
        /// Экземпляр VkApi
        /// </summary>
        IVkApi vkApi { get; set; }

        /// <summary>
        /// Проверяем соответствует ли объект данной команде
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        bool IsMatch(object update, DatabaseContext db);

        /// <summary>
        /// Обрабатываем команду
        /// </summary>
        /// <param name="vkApi"></param>
        /// <param name="update"></param>
        Task Handle(object update, DatabaseContext db);
    }
}
