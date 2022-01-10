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
    public class SendFakultyCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public SendFakultyCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            var keyboard = new KeyboardBuilder().SetInline(true);
            var json = JObject.Parse(eventbody.Payload);
            int course = (int)json["course"];
            var faculties = db.Groups.Where(x => x.Course == course).Select(x => x.Faculty).Distinct().AsEnumerable().Chunk(2);
            foreach (var facultygroup in faculties)
            {
                foreach (var faculty in facultygroup)
                {
                    keyboard.AddButton(new MessageKeyboardButtonAction()
                    {
                        Label = faculty,
                        Type = KeyboardButtonActionType.Callback,
                        Payload = "{\"course\":" + course + ",\"faculty\":\"" + faculty + "\"}",
                    });
                }
                keyboard.AddLine();
            }
            long? msgId = db.Users.Where(x => x.UserId == eventbody.UserId).First().msgId;
            await vkApi.Messages.EditAsync(new MessageEditParams()
            {
                Message = "Выберите свой факультет",
                MessageId = msgId,
                PeerId = (long)eventbody.PeerId,
                Keyboard = keyboard.Build(),
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            if (eventbody != null)
            {
                var json = JObject.Parse(eventbody.Payload);
                if (json["course"] != null && json["faculty"] == null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
