using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;

namespace Timetable.BotCore.Commands.TextMessage.UserCommands
{
    public class WeekCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }


        public Task Handle(object update, DatabaseContext db)
        {
            return Task.CompletedTask;
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg != null && msg.Text.ToLower().Contains("текущая неделя") ||
                                  msg.Text.ToLower().Contains("следующая неделя");
        }
    }
}
