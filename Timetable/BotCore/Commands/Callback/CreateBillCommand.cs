using Newtonsoft.Json.Linq;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.GroupUpdate;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.Callback
{
    public class CreateBillCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public CreateBillCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            var user = db.Users.Where(x => x.UserId == eventbody.UserId).FirstOrDefault();
            using (QiwiPayment client = new QiwiPayment())
            {
                try
                {
                    var keyboard = new KeyboardBuilder().SetInline(true);
                    var data = await client.CreatePayment();
                    keyboard.AddButton(new MessageKeyboardButtonAction()
                    {
                        Label = "💳 Оплатить",
                        Type = KeyboardButtonActionType.OpenLink,
                        Payload = "{\"createbill\":1}",
                        Link = new Uri(data.PayUrl)
                    });
                    keyboard.AddLine();
                    keyboard.AddButton(new MessageKeyboardButtonAction()
                    {
                        Label = "🔄 Проверить",
                        Type = KeyboardButtonActionType.Callback,
                        Payload = "{\"checkStatus\":\"" + data.BillId + "\"}",
                    }, KeyboardButtonColor.Primary);
                    keyboard.AddLine();
                    keyboard.AddButton(new MessageKeyboardButtonAction()
                    {
                        Label = "⛔ Отменить",
                        Type = KeyboardButtonActionType.Callback,
                        Payload = "{\"reject\":\"" + data.BillId + "\"}"
                    }, KeyboardButtonColor.Negative);
                    long MsgId = await vkApi.Messages.SendAsync(new MessagesSendParams()
                    {
                        RandomId = ConcurrentRandom.Next(),
                        UserId = eventbody.UserId,
                        Message = "💰 Счёт на оплату сформирован\n" +
                                  "⚠ У вас есть 20 минут на оплату, далее счёт будет отменён\n" +
                                  "ℹ После оплаты нажмите кнопку «Проверить», чтобы активировать подписку\n" +
                                  "👤 Если у вас возникли проблемы с оплатой, пишите сюда: https://vk.com/topic-208050569_48329662 \n" +
                                  $"⚙ Ваш Id: " + data.BillId,
                        Keyboard = keyboard.Build(),
                    });
                    user.MsgId = MsgId;
                    await db.SaveChangesAsync();
                }
                catch (HttpRequestException)
                {
                    await vkApi.Messages.SendAsync(new MessagesSendParams()
                    {
                        RandomId = ConcurrentRandom.Next(),
                        UserId = eventbody.UserId,
                        Message = "⛔ Произошла ошибка при формировании счета, попробуйте ещё раз"
                    });
                }
            }
            await vkApi.Messages.SendMessageEventAnswerAsync(eventbody.EventId, (long)eventbody.UserId, (long)eventbody.PeerId, null);
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            if (eventbody != null)
            {
                var json = JObject.Parse(eventbody.Payload);
                if (json["command"] != null && json["command"].ToString() == "createbill")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
