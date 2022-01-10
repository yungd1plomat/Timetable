using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда для удаления администратора
    /// </summary>
    public class RemoveAdminCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public RemoveAdminCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            try
            {
                var arguments = msg.Text.Split(' ');
                string screen_name = arguments[1].Split('/').Last(); // https://vk.com/musin007, получаем screen_name - musin007

                if (!long.TryParse(screen_name, out long userid))
                {
                    userid = vkApi.Users.Get(new string[] { screen_name })[0].Id; // Получаем id юзера (если задан адрес страницы)
                }

                var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
                if (user != null)
                {
                    user.admin = false;
                    user.Subscribtion = DateTime.Now.AddDays(1);
                    await vkApi.Messages.SendAsync(new MessagesSendParams()
                    {
                        Message = "🥺 Вас сняли с администратора :(\nНапишите «Начать», чтобы вернуть обычное меню",
                        UserId = userid,
                        RandomId = Bot.rnd.Next(),
                    });
                    await db.SaveChangesAsync();
                }
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    Message = $"☑ Пользователь {screen_name} успешно снят с администратора",
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
                if (text.Contains("/removeadmin") && user.admin == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
