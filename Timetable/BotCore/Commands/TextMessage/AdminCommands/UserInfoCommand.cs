using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда получения информации о пользователе
    /// </summary>
    public class UserInfoCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public UserInfoCommand(IVkApi vkApi)
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
                string message = "👤 Информации о пользователе не найдено";
                if (user != null)
                {
                    var Admin = user.Admin.HasValue && user.Admin.Value;
                    var expires = user.Subscribtion.HasValue ? user.Subscribtion.Value.ToString("HH:mm dd.MM.yyyy") : null;
                    var billId = user.BillId;
                    var group = user.Group.GroupName;
                    message = "👤 Информация о пользователе:\n\n" +
                              $"🔶 Админ: {Admin}\n" +
                              $"💰 Подписка: {expires}\n" +
                              $"🧾 Последний чек: {billId}\n" +
                              $"👥 Группа: {group}";
                }
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    Message = message,
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
                if (msg.Text.ToLower().Contains("/info") && user.Admin == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
