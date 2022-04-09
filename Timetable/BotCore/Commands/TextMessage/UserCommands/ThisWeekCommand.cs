using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;

namespace Timetable.BotCore.Commands.TextMessage.UserCommands
{
    public class ThisWeekCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public Task Handle(object update, DatabaseContext db)
        {
            
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            throw new NotImplementedException();
        }
    }
}
