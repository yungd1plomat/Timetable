using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;


namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда для получения списка команд админа (справка)
    /// </summary>
    public class HelpCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public HelpCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        private const string HelpMessage = "Команды админа:\n" +
                                           "1) /addadmin https://vk.com/durov - добавить нового админа (ссылка на страницу)\n" +
                                           "2) /removeadmin https://vk.com/durov - удалить админа (ссылка на страницу)\n" +
                                           "3) /add https://vk.com/durov 30 - добавить дней подписки (ссылка на страницу и кол-во дней)\n" +
                                           "4) /delete https://vk.com/durov 30 - убрать дней подписки (ссылка на страницу и кол-во дней)\n" +
                                           "5)/info https://vk.com/durov - Информация о пользователе\n";
        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid  = msg.FromId.Value;
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = HelpMessage,
                UserId = userid,
                RandomId = msg.RandomId,
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg != null)
            {
                var user = db.Users.Where(x => x.UserId == msg.FromId).FirstOrDefault();
                string text = msg.Text.ToLower();
                if (text.Contains(@"¯\_(ツ)_/¯") && user.admin == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
