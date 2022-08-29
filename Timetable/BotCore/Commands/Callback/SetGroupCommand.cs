using Newtonsoft.Json.Linq;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.Callback
{
    public class SetGroupCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public SetGroupCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            var json = JObject.Parse(eventbody.Payload);
            long groupId = (long)json["groupId"];
            var group = db.Groups.Where(x => x.GroupIdentifyId == groupId).FirstOrDefault();
            var user = db.Users.Where(x => x.UserId == (long)eventbody.UserId).FirstOrDefault();

            try
            {
                await vkApi.Messages.DeleteAsync(new ulong[] { (ulong)user.MsgId }, false, null, true); // Удаляем сообщение с выбором группы
            } catch { }

            user.Group = group;
            user.MsgId = null;
            await db.SaveChangesAsync();

            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                RandomId = Bot.rnd.Next(),
                UserId = (long)eventbody.UserId,
                Message = "✅ Вы успешно установили группу " + group.GroupName
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var eventbody = update as MessageEvent;
            if (eventbody != null)
            {
                var json = JObject.Parse(eventbody.Payload);
                if (json["groupId"] != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
