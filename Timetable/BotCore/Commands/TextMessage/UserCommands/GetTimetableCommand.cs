using System.Text.RegularExpressions;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда получения расписания
    /// </summary>
    public class GetTimetableCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public GetTimetableCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            string message = "none";
            if (user.Group == null)
            {
                message = "❌ Для начала установите свою группу.Если у вас нету меню напишите «Начать» ❌";
            } 
            else
            {
                DateTime date = DateTime.MinValue;
                var text = new string(msg.Text.ToLower().ToCharArray().Where(e => char.IsLetter(e)).ToArray());
                switch (text)
                {
                    case "сегодня":
                        {
                            message = $"📕 Расписание на сегодня:\n\n";
                            date = DateTime.Today.Date;
                            break;
                        }
                    case "завтра":
                        {
                            message = $"📕 Расписание на завтра:\n\n";
                            date = DateTime.Today.AddDays(1);
                            break;
                        }
                    case "послезавтра":
                        {
                            message = $"📕 Расписание на послезавтра:\n\n";
                            date = DateTime.Today.AddDays(2);
                            break;
                        }
                }
                var lessons = db.Lessons.Where(x => x.Group == user.Group && x.StartTime.Date == date);
                if (lessons.Any())
                {
                    foreach (var lesson in lessons)
                    {
                        message += lesson.ToString();
                        message += "\r\n\n";
                    }
                } 
                else
                {
                    message = "😋 Нет занятий 😋";
                }
            }
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                UserId = userid,
                RandomId = Bot.rnd.Next(),
                Message = message,
                DontParseLinks = true, //https://vk.com/topic-208050569_48189901?post=14
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var message = update as Message;
            if (message != null && (message.Text.ToLower().Contains("сегодня") 
                                 || message.Text.ToLower().Contains("завтра")
                                 || message.Text.ToLower().Contains("послезавтра")))
            {
                return true;
            }
            return false;
        }
    }
}
