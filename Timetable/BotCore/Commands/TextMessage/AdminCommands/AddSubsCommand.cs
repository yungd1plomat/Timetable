using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда для добавления дней подписки
    /// </summary>
    public class AddSubsCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public AddSubsCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            try
            {
                var arguments = msg.Text.Split(' ');
                string command = arguments[0];

                string screen_name = arguments[1].Split('/').Last(); // https://vk.com/musin007, получаем screen_name - musin007

                if (!long.TryParse(screen_name, out long userid))
                {
                    userid = vkApi.Users.Get(new string[] { screen_name })[0].Id; // Получаем id юзера (если задан адрес страницы)
                }

                int days = int.Parse(arguments[2]);

                var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();


                if (user != null)
                {
                    var expires = user.Subscribtion;
                    if (expires.HasValue && expires.Value > DateTime.Now)
                    {
                        user.Subscribtion = expires.Value.AddDays(days);
                    }
                    else
                    {
                        user.Subscribtion = DateTime.Now.AddDays(days);
                    }
                    await vkApi.Messages.SendAsync(new MessagesSendParams()
                    {
                        Message = $"➕ Вам добавили {days} дней подписки!",
                        UserId = userid,
                        RandomId = Bot.rnd.Next(),
                    });
                }
                else
                {
                    await db.Users.AddAsync(new BotUser()
                    {
                        UserId = userid,
                        Subscribtion = DateTime.Now.AddDays(days),
                    });
                }
                await db.SaveChangesAsync();
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    Message = $"☑ Пользователю {screen_name} успешно добавлено {days} дней",
                    UserId = msg.FromId.Value,
                    RandomId = Bot.rnd.Next(),
                });
            }
            catch
            {
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    Message = "Произошла ошибка при выполнении команды, проверьте синтаксис",
                    UserId = msg.FromId.Value,
                    RandomId = Bot.rnd.Next(),
                });
            }
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg != null)
            {
                var user = db.Users.Where(x => x.UserId == msg.FromId).FirstOrDefault();
                string text = msg.Text.ToLower();
                if (text.Contains("/add")   && user.admin == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
