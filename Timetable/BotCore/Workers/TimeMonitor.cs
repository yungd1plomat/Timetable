using Microsoft.EntityFrameworkCore;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;


namespace Timetable.BotCore.Workers
{
    public class TimeMonitor : IMonitor
    {
        public IVkApi _vkApi { get; set; }
        public Timer timer { get; set; }
        public IEnumerable<TimeSpan> _intervals { get; set; }

        private readonly ILogger _logger;

        /// <summary>
        /// Нужно для сообщений Вконтакте
        /// </summary>
        private readonly Random rnd = new Random();
        
        /// <summary>
        /// За сколько минут придет уведомление
        /// </summary>
        private readonly int beforeMinutes = 15;

        /// <summary>
        /// Время в которое расписание обновится
        /// </summary>
        private readonly TimeSpan updateTime = new TimeSpan(0, 10, 0); // Время в которое расписание обновится

        /// <summary>
        /// Первый запуск
        /// </summary>
        private bool FistStart = true;


        public TimeMonitor(IVkApi api, ILogger _logger)
        {
            _vkApi = api;
            this._logger = _logger;
            _intervals = new List<TimeSpan>()
            {
                new TimeSpan(8, 0, 0),
                new TimeSpan(9, 50, 0),
                new TimeSpan(12, 20, 0),
                new TimeSpan(14,10, 0),
                new TimeSpan(16, 0, 0),
                new TimeSpan(17, 50, 0),
                new TimeSpan(19, 30, 0),
            };
        }

        public async void CheckTime(object obj)
        {
            // Просчитываем будущее время
            var futureTime = DateTime.Now.AddMinutes(beforeMinutes);
            _logger.LogInformation("Проверка времени " + futureTime.ToString("HH:mm dd.MM.yyyy"));

            // Смотрим соответствует ли текущее (будущее) время
            // времени начала пар
            using (DatabaseContext db = new DatabaseContext())
            {

                // Тут будем хранить все занятия, которые начнутся через n - минут
                List<Lesson> allLessons = new List<Lesson>();
                foreach (var interval in _intervals)
                {
                    // Если текущее (будущее) время соответствует начале пары
                    if (interval.TimeEquals(futureTime.TimeOfDay))
                    {
                        var lessons = db.Lessons.Include(x => x.Group).ToList().Where(x => x.StartTime.DateEquals(futureTime));

                        allLessons.AddRange(lessons);
                    }
                }

                if (allLessons.Any())
                {
                    _logger.LogInformation("Время прошло проверку " + futureTime.ToString("HH:mm dd.MM.yyyy"));
                    SendNotifications(allLessons);
                }

                // Если текущее время соответствует времени обновления
                // или если это первый запуск (бд пуста)
                if (updateTime.TimeEquals(futureTime.TimeOfDay) || (FistStart && !db.Lessons.Any()))
                {
                    FistStart = false;
                    UpdateTimetable();
                }
            }
        }

        public async void SendNotifications(IEnumerable<Lesson> lessons)
        {
            _logger.LogInformation("Начата рассылка в " + DateTime.Now.ToString("HH:mm dd.MM.yyyy"));
            using (DatabaseContext db = new DatabaseContext())
            {
                foreach (var lesson in lessons)
                {
                    // Берём только userid
                    var chunksUsers = db.Users.Where(x => x.Group == lesson.Group && x.Subscribtion.HasValue)
                                              .ToList()
                                              .Where(x => x.Subscribtion > DateTime.Now)
                                              .Select(x => x.UserId).Chunk(100);
                    if (chunksUsers.Any())
                    {
                        // Сообщение которое получит пользователь
                        string message = $"🔔 Через {beforeMinutes} минут у вас начинается занятие:\r\n\n{lesson.ToShortString()}";

                        // Рассылаем по 100 юзеров
                        foreach (var users in chunksUsers)
                        {
                            try
                            {
                                await _vkApi.Messages.SendToUserIdsAsync(new MessagesSendParams()
                                {
                                    UserIds = users,
                                    Message = message,
                                    RandomId = rnd.Next(),
                                });
                            }
                            catch { } // Запрет сообщений
                            //await Task.Delay(1000); // На всякий случай, ограничение вк апи
                        }
                        _logger.LogInformation("Уведомили группу {0}", lesson.Group.GroupName);
                    }
                }
            }
        }

        public void StartMonitoring()
        {
            TimerCallback timerCallback = new TimerCallback(CheckTime);
            timer = new Timer(timerCallback, null, 0, 60000); // Чекаем время каждую минуту

            GC.KeepAlive(timer); // Чтобы сборщик не удалил :D
        }

        public async void UpdateTimetable()
        {
            // using позволяет работать с ассинхронностью
            try
            {
                using (Parser parser = new Parser())
                {
                    parser.ClearCache();
                    parser.UpdateGroups();
                    await parser.UpdateTimetable();
                }
            } catch { }
        }

    }
}
