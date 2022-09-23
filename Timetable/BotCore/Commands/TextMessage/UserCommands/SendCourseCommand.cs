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
    /// Команда для отправления существующих курсов
    /// </summary>
    public class SendCourseCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public SendCourseCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }


        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var courses = db.Groups.Select(x => x.Course).Distinct();
            var keyboard = new KeyboardBuilder().SetInline(true);
            foreach (var course in courses)
            {
                keyboard.AddButton(new MessageKeyboardButtonAction()
                {
                    Label = course.ToString(),
                    Type = KeyboardButtonActionType.Callback,
                    Payload = "{\"course\":" + course + "}",
                });
                keyboard.AddLine();
            }
            long MsgId = await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = "Выберите свой курс",
                Keyboard = keyboard.Build(),
                UserId = userid,
                RandomId = Bot.rnd.Next()
            });
            db.Users.Where(x => x.UserId == userid).First().MsgId = MsgId;
            await db.SaveChangesAsync();
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg != null && msg.Text.ToLower().Contains("установить группу");
        }
    }
}
