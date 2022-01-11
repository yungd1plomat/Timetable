using Newtonsoft.Json.Linq;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model.GroupUpdate;
using VkNet.Model;
using VkNet.Enums.SafetyEnums;

namespace Timetable.BotCore.Commands.Callback
{
    public class CheckBillCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public CheckBillCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            var json = JObject.Parse(eventbody.Payload);
            var billId = json["checkStatus"].ToString();
            var user = db.Users.Where(x => x.UserId == (long)eventbody.UserId).FirstOrDefault();
            string message = "🇺🇦 Вам уже продлена подписка!";
            if (user.BillId != billId)
            {
                using (QiwiPayment client = new QiwiPayment())
                {
                    try
                    {
                        var status = await client.CheckPayment(billId);
                        switch (status)
                        {
                            case PaymentStatus.NONE:
                                {
                                    throw new HttpRequestException();
                                    break;
                                }
                            case PaymentStatus.WAITING:
                                {
                                    message = "⏳ Ожидаем оплаты..";
                                    break;
                                }
                            case PaymentStatus.PAID:
                                {
                                    message = "🔥 Ваша подписка была продлена на 30 дней";
                                    var expire = user.Subscribtion;
                                    if (expire < DateTime.Now) // Если подписка истекла, то добавляем к сегодняшнему дню
                                    {
                                        user.Subscribtion = DateTime.Now.AddDays(30);
                                    }
                                    else // Если подписка активна, добавляем ещё 30 дней
                                    {
                                        user.Subscribtion = expire.Value.AddDays(30);
                                    }
                                    long? msgId = user.msgId;
                                    user.msgId = null;
                                    user.BillId = billId; // Id последнего платежа, если вдруг сообщение с проверкой не удалится
                                    await db.SaveChangesAsync();
                                    if (msgId != null)
                                    {
                                        await vkApi.Messages.DeleteAsync(new ulong[] { (ulong)msgId }, false, null, true);
                                    }
                                    break;
                                }
                            case PaymentStatus.EXPIRED or PaymentStatus.REJECTED:
                                {
                                    message = "❗ Время оплаты счёта истекло, либо он был отклонён";
                                    if (user.msgId != null)
                                    {
                                        await vkApi.Messages.DeleteAsync(new ulong[] { (ulong)user.msgId }, false, null, true);
                                        user.msgId = null;
                                        await db.SaveChangesAsync();
                                    }
                                    break;
                                }
                        }
                    }
                    catch (HttpRequestException)
                    {
                        message = "‼ Произошла ошибка при проверке платежа, попробуйте ещё раз";
                    }
                }
                await vkApi.Messages.SendMessageEventAnswerAsync(eventbody.EventId, (long)eventbody.UserId, (long)eventbody.PeerId, new EventData()
                {
                    Type = MessageEventType.SnowSnackbar,
                    Text = message
                });
            }
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            if (eventbody != null)
            {
                var json = JObject.Parse(eventbody.Payload);
                if (json["checkStatus"] != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
