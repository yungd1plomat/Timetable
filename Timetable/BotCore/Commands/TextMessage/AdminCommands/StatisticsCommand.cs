using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда получания статистики
    /// </summary>
    public class StatisticsCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public StatisticsCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var users = db.Users.Select(x => new {Subscription = x.Subscribtion, BillId = x.BillId});

            int usercount = users.Count();
            int subscribeUsers = users.Where(x => x.Subscription > DtExtensions.LocalTimeNow()).Count();
            int paidUsers = users.Where(x => x.BillId != null && x.Subscription > DtExtensions.LocalTimeNow()).Count();
            double balance = 0;
            using (QiwiPayment client = new QiwiPayment())
            {
                try
                {
                    balance = await client.GetBalance();
                }
                catch { }
            }
            string message = $"👥 Всего пользователей: {usercount}\n" +
                             $"🧾 Пользователей с подпиской: {subscribeUsers}\n" +
                             $"♠ Купили подписку (приблизительно): {paidUsers}\n" +
                             $"♣ С тестовой подпиской: {subscribeUsers - paidUsers}\n" +
                             $"💰 Баланс кошелька: {balance}";
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = message,
                UserId = userid,
                RandomId = ConcurrentRandom.Next(),
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg != null)
            {
                var user = db.Users.Where(x => x.UserId == msg.FromId).FirstOrDefault();
                if (msg.Text.ToLower().Contains("статистика") && user.Admin == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
