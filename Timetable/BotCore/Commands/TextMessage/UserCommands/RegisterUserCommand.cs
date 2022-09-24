using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models;
using VkNet.Abstractions;
using VkNet.Model;


namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда регистрации юзера (добавление в бд)
    /// </summary>
    public class RegisterUserCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public RegisterUserCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            if (!db.Users.Select(x => x.UserId).Contains(userid))
            {
                await db.Users.AddAsync(new BotUser()
                {
                    UserId = userid,
                    Subscribtion = DateTime.MaxValue,
                    Timer = 15,
                });
                db.SaveChanges();
            }
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg != null && !db.Users.Select(x => x.UserId).Contains(msg.FromId.Value);
        }
    }
}
