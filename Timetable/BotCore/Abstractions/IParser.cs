using Timetable.Helpers;

namespace Timetable.BotCore.Abstractions
{
    /// <summary>
    /// Экземпляр парсера для скрепинга расписания с asu.bspu.ru
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Контекст бд
        /// </summary>
        DatabaseContext db { get; set; }

        /// <summary>
        /// Логгер
        /// </summary>
        ILogger _logger { get; set; }

        /// <summary>
        /// Создание хеша MD5 из строки
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string CreateMD5(string input);

        /// <summary>
        /// Обновить группы
        /// Метод должен быть синхронным, т.к
        /// после обновления групп
        /// должно запуститься обновление расписания
        /// </summary>
        void UpdateGroups();

        /// <summary>
        /// Обновить расписание занятий у групп
        /// Метод будет ассинхронным 
        /// </summary>
        Task UpdateTimetable();

        /// <summary>
        /// Удалить старое расписание
        /// </summary>
        void ClearCache();
    }
}
