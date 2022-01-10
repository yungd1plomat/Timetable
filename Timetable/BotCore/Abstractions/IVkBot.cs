using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.GroupUpdate;

namespace Timetable.BotCore.Abstractions
{
    /// <summary>
    /// Экземпляр самого бота
    /// </summary>
    public interface IVkBot
    {
        /// <summary>
        /// Экземпляр VkApi для работы с апи вк
        /// </summary>
        IVkApi _vkApi { get; set; }

        /// <summary>
        /// Экземпляр Monitor, для слежения за временем и
        /// уведомления пользователей о начале занятий
        /// </summary>
        IMonitor _monitor { get; set; }

        /// <summary>
        /// Все текстовые команды Бота для обработки текстовых сообщений
        /// </summary>
        IVkBotCommand[] vkBotTextCommands { get; set; }

        /// <summary>
        /// Все callback команды Бота для обработки нажатия на кнопки
        /// </summary>
        IVkBotCommand[] vkBotCallbackCommands { get; set; }

        /// <summary>
        /// Устанавливает токен авторизации для VkApi
        /// </summary>
        /// <param name="accessToken"></param>
        void Authorize(string accessToken);

        /// <summary>
        /// Обрабатываем callback эвент (нажатие по инлайн кнопке и т.д)
        /// </summary>
        /// <param name="msgEvent"></param>
        void RegisterEvent(MessageEvent msgEvent);

        /// <summary>
        /// Обрабатываем текстовое сообщение
        /// </summary>
        /// <param name="msg"></param>
        void RegisterMessage(Message msg);
    }
}
