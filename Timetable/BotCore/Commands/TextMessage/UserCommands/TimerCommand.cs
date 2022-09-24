using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;


namespace Timetable.BotCore.Commands.TextMessage
{
    public class TimerCommand : IVkBotCommand
    {
        public IVkApi vkApi { get;set; }

        public TimerCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            user.Timer = int.TryParse(msg.Text, out int result) ? result : null;
            await db.SaveChangesAsync();
            string message = user.Timer is null ? "🕐 Введите время (в минутах) за которое вам придет уведомление о паре.\n" +
                                                  "⏹ Для отключения уведомлений введите 0." : 
                             user.Timer == 0 ? "☑️ Уведомления были выключены." :
                                               $"☑️ Таймер успешно установлен. Теперь вы будете получать уведомления за {result} минут до пары";
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                RandomId = ConcurrentRandom.Next(),
                UserId = userid,
                Message = message,
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg is null)
                return false;
            var user = db.Users.Where(x => x.UserId == msg.FromId).FirstOrDefault();
            return msg.Text.ToLower().Contains("таймер") ||
                   (int.TryParse(msg.Text, out int result) &&
                   user.Timer is null);
        }
    }
}
