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
                var expires = new DateTime(2022, 1, 20, 18, 0, 0);
                if (DateTime.Now > expires)
                {
                    expires = DateTime.Now.AddDays(3);
                }
                await db.Users.AddAsync(new BotUser()
                {
                    UserId = userid,
                    Subscribtion = expires,
                });
                db.SaveChanges();
            }
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg != null && !db.Users.Select(x => x.UserId).Contains(msg.FromId.Value))
            {
                return true;
            }
            return false;
        }
    }
}
