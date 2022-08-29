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
    public class SendGroupsCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public SendGroupsCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            var json = JObject.Parse(eventbody.Payload);
            string faculty = (string)json["faculty"];
            int course = (int)json["course"];
            int page = json["page"] != null ? (int)json["page"] : 0;
            var groups = db.Groups.Where(x => x.Faculty == faculty && x.Course == course)  // Выбираем группы с заданным факультетом и курсом
                                  .OrderBy(x => x.GroupName) // Сортируем (чтобы не было путаницы)
                                  .AsEnumerable()
                                  .Chunk(8) // Делим по 8 групп (в вк можно отправить только 10 кнопок - 2 кнопки навигации и 8 групп)
                                  .ToArray();
            var keyboard = new KeyboardBuilder().SetInline(true);

            var chunkGroups = groups[page].Chunk(2); // В ряду можно добавить только 2 кнопки
                                                     // Ограничения вк - 2 кнопки в ряд и всего 10 рядов

            foreach (var chunkgroup in chunkGroups)
            {
                foreach (var group in chunkgroup)
                {
                    keyboard.AddButton(new MessageKeyboardButtonAction()
                    {
                        Label = group.GroupName,
                        Type = KeyboardButtonActionType.Callback,
                        Payload = "{\"groupId\":" + group.GroupIdentifyId + "}",
                    });
                }
                keyboard.AddLine();
            }

            if (page != 0) // На первой странице нет смысла добавлять кнопку назад, т.к её нету
            {
                keyboard.AddButton(new MessageKeyboardButtonAction()
                {
                    Label = "⬅️",
                    Type = KeyboardButtonActionType.Callback,
                    Payload = "{\"faculty\":\"" + faculty + "\", \"page\":" + (page - 1) + ", \"course\": " + course + "}",
                });
            }

            if (page != (groups.Count() - 1)) // На последней странице нет смысла добавлять кнопку вперед, т.к её нету
            {
                keyboard.AddButton(new MessageKeyboardButtonAction()
                {
                    Label = "➡️",
                    Type = KeyboardButtonActionType.Callback,
                    Payload = "{\"faculty\":\"" + faculty + "\", \"page\":" + (page + 1) + ", \"course\": " + course + "}",
                });
            }

            long? MsgId = db.Users.Where(x => x.UserId == eventbody.UserId).First().MsgId;

            await vkApi.Messages.EditAsync(new MessageEditParams()
            {
                Message = $"[{page}] Выберите свою группу",
                MessageId = MsgId,
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
                if (json["course"] != null && json["faculty"] != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
