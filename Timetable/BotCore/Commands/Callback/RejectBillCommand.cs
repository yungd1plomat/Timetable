using Newtonsoft.Json.Linq;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model.GroupUpdate;
using VkNet.Model;
using VkNet.Enums.SafetyEnums;

namespace Timetable.BotCore.Commands.Callback
{
    public class RejectBillCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public RejectBillCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            var json = JObject.Parse(eventbody.Payload);
            var billId = json["reject"].ToString();
            var user = db.Users.Where(x => x.UserId == eventbody.UserId).FirstOrDefault();
            string message = "Error";
            using (QiwiPayment payment = new QiwiPayment())
            {
                try
                {
                    await payment.RejectPayment(billId);
                    if (user.MsgId != null)
                    {
                        await vkApi.Messages.DeleteAsync(new ulong[] { (ulong)user.MsgId }, false, null, true);
                        user.MsgId = null;
                        await db.SaveChangesAsync();
                    }
                    message = "🌝 Счёт успешно отменён";
                    
                }
                catch (HttpRequestException)
                {
                    message = "⛔ Произошла ошибка при отмене счёта, попробуйте ещё раз";
                }
            }
            await vkApi.Messages.SendMessageEventAnswerAsync(eventbody.EventId, (long)eventbody.UserId, (long)eventbody.PeerId, new EventData()
            {
                Type = MessageEventType.SnowSnackbar,
                Text = message
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            if (eventbody != null)
            {
                var json = JObject.Parse(eventbody.Payload);
                if (json["reject"] != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
