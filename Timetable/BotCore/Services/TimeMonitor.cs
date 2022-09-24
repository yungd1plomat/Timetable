using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;


namespace Timetable.BotCore.Workers
{
    public class TimeMonitor : IMonitor
    {
        private const string vkScript = "var i = 0;\r\nwhile (i != data.length) {\t\r\nvar msg = data[i][0];\r\n\tvar arr = data[i];\r\n\tvar userIds = arr.slice(1, arr.lenght);\r\n\tAPI.messages.send({\"user_ids\": userIds, \"random_id\":randomIds[i], \"message\": msg });\r\n\ti = i + 1;\r\n}";

        public IVkApi _vkApi { get; set; }

        public Timer timer { get; set; }

        public IEnumerable<TimeSpan> Intervals { get; set; }


        private readonly ILogger _logger;
        
        /// <summary>
        /// Время в которое расписание обновится
        /// </summary>
        private readonly TimeSpan updateTime = new TimeSpan(0, 0, 0); // Время в которое расписание обновится

        /// <summary>
        /// Первый запуск
        /// </summary>
        private bool FirstStart = true;


        public TimeMonitor(IVkApi api, ILogger _logger)
        {
            _vkApi = api;
            this._logger = _logger;
            Intervals = new List<TimeSpan>()
            {
                new TimeSpan(8, 0, 0),
                new TimeSpan(9, 50, 0),
                new TimeSpan(12, 20, 0),
                new TimeSpan(14, 10, 0),
                new TimeSpan(16, 0, 0),
                new TimeSpan(17, 50, 0),
                new TimeSpan(19, 30, 0),
            };
        }

        public async void CheckTime(object obj)
        {
            var currentTime = DtExtensions.LocalTimeNow();
            _logger.LogInformation($"Проверка времени {currentTime}");
            using (DatabaseContext db = new DatabaseContext())
            {
                var users = db.Users.Where(x => x.Timer != null &&
                                                x.Timer != 0).ToList();
                var lessons = db.Lessons.Include(x => x.Group).ToList();
                Dictionary<string, List<long>> userMessages = new ();
                foreach (var user in users)
                {
                    if (user.Timer == null ||
                        user.Timer == 0)
                        continue;
                    // Просчитываем будущее время
                    var futureTime = currentTime.AddMinutes(user.Timer.Value);
                    if (!Intervals.Any(x => x.TimeEquals(futureTime.TimeOfDay)))
                        continue;
                    var userLessons = lessons.Where(x => x.Group == user.Group &&
                                                         x.StartTime.DateEquals(futureTime))
                                             .Select(x => x.ToShortString());
                    if (!userLessons.Any())
                        continue;
                    _logger.LogInformation($"У пользователя {user.UserId} начинается занятие через {user.Timer} минут");
                    string message = string.Format("🔔 Через {0} минут у вас начинается занятие:\\r\\n\\n{1}", user.Timer, string.Join("\\n", userLessons));
                    if (userMessages.ContainsKey(message))
                        userMessages[message].Add(user.UserId);
                    else
                        userMessages.Add(message, new List<long>() { user.UserId });
                }
                var codes = PackToCodes(userMessages);
                await SendNotifications(codes);
                // Если текущее время соответствует времени обновления
                // или если это первый запуск (бд пуста)
                if (updateTime.TimeEquals(currentTime.TimeOfDay) || (FirstStart && !db.Lessons.Any()))
                {
                    FirstStart = false;
                    UpdateTimetable();
                }
            }
        }

        public async Task SendNotifications(IEnumerable<string> codes)
        {
            foreach (var code in codes)
            {
                try
                {
                    await _vkApi.Execute.ExecuteAsync(code);
                    _logger.LogInformation($"Успешно выполнили код:\n\n{code}");
                } catch (Exception ex)
                {
                    _logger.LogError($"Ошибка при execute, код:\n\n {code}\n\n текст: {ex.Message}");
                }
                await Task.Delay(320);
            }
        }

        public IEnumerable<string> PackToCodes(Dictionary<string, List<long>> userMessages)
        {
            /*var randomIds = [128923, 12324];
              var data = [
                 ["hi1", 13337, 137778129],
                 ["test1", 12222, 137778129]
              ];
              var i = 0;
              while (i != data.length) { 
                 var msg = data[i][0];
                 var arr = data[i];
                 var userIds = arr.slice(1, arr.lenght);
                 API.messages.send({"user_ids": userIds, "random_id":randomIds[i], "message": msg });
                i = i + 1;
              };*/

            List<string> chunkCodes = new List<string>();
            // Максимум 25 вызовов api в 1 execute
            var chunkMessages = userMessages.Chunk(25);
            StringBuilder sb = new StringBuilder();
            foreach (var messages in chunkMessages)
            {
                var randomIds = Enumerable.Repeat(0, chunkMessages.Count()).Select(x => ConcurrentRandom.Next());
                sb.Append("var randomIds = [");
                sb.Append(string.Join(", ", randomIds));
                sb.Append("];\r\n");
                sb.Append("var data = [\r\n");
                foreach (var message in messages)
                {
                    sb.Append("\t[\"");
                    sb.Append(message.Key);
                    sb.Append("\", ");
                    sb.Append(string.Join(", ", message.Value));
                    sb.Append("],\r\n");
                }
                sb.Append("];\n");
                sb.Append(vkScript);
                chunkCodes.Add(sb.ToString());
                _logger.LogDebug("Generated code:\n" + sb.ToString());
                sb.Clear();
            }
            return chunkCodes;
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
                GC.Collect();
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error when parsing or updating timetable");
            }
        }
    }
}
