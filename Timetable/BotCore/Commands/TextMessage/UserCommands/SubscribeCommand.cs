using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда подписка
    /// </summary>
    public class SubscribeCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }
        private MessageKeyboard subscribekey { get; set; } // Кнопки продления

        public SubscribeCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
            subscribekey = new KeyboardBuilder().AddButton(new MessageKeyboardButtonAction()
                                                {
                                                    Label = "💵 Продлить",
                                                    Type = KeyboardButtonActionType.Callback,
                                                    Payload = "{\"command\":\"createbill\"}",
                                                }, KeyboardButtonColor.Positive)
                                                .SetInline(true)
                                                .Build();
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            DateTime? expiration = db.Users.Where(x => x.UserId == userid).First().Subscribtion;
            string message;
            if (expiration.HasValue && expiration > DtExtensions.LocalTimeNow())
            {
                message = $"💎 Ваша подписка активна до {expiration.Value.ToString("HH:mm dd.MM.yyyy")}";
            }
            else
            {
                message = $"❌ Ваша подписка закончилась";
            }
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = message,
                RandomId = ConcurrentRandom.Next(),
                UserId = userid,
                Keyboard = subscribekey,
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg != null)
            {
                long userid = msg.FromId.Value;
                DateTime? expiration = db.Users.Where(x => x.UserId == userid).First().Subscribtion;
                if (msg.Text.ToLower().Contains("подписка") || !expiration.HasValue || expiration.Value < DtExtensions.LocalTimeNow())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
