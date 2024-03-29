﻿using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;


namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда поиска указанного текста
    /// </summary>
    public class FindCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public FindCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            var str = msg.Text.ToLower();
            if (str.Contains("поиск"))
            {
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    UserId = (long)msg.FromId,
                    RandomId = ConcurrentRandom.Next(),
                    Message = "🔍 Введите строку поиска (название предмета,преподавателя или времени) и мы найдем ближайшие пары",
                });
                return;
            }
            string message = "❌ Для начала установите свою группу.Если у вас нету меню напишите «Начать» ❌";
            if (user.Group != null)
            {
                message = "🕵 По вашему запросу ничего не найдено";
                var lessons = db.Lessons.Where(x => x.Group == user.Group).ToList()
                                        .Where(x => x.Teacher.ToLower().Contains(str) ||
                                                     x.StartTime.ToString("HH:mm dd.MM.yyyy").Contains(str) ||
                                                     x.Subject.ToLower().Contains(str))
                                        .OrderBy(x => x.StartTime)
                                        .Take(5);
                if (lessons.Any())
                {
                    message = "ℹ Вот что мне удалось найти:\n\n";
                    foreach (var lesson in lessons)
                    {
                        message += lesson.ToLongString();
                        message += "\r\n\n";
                    }
                    message += "☝* показаны только первые 5 результатов";
                }
            }
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                UserId = userid,
                Message = message,
                RandomId = ConcurrentRandom.Next(),
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg.Text != null && msg.Text.Length > 0;
        }
    }
}
