using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Utils;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Если получен attachment (стикер, видео и т.д), то отвечает какой нибудь гифкой
    /// </summary>
    public class StickerCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        private readonly string[] gifs;

        public StickerCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
            gifs = new string[]
            {
                "video-208050569_456239018",
                "video-208050569_456239019",
                "video-208050569_456239020",
                "video-208050569_456239021",
                "video-208050569_456239022",
                "video-208050569_456239023",
                "video-208050569_456239024"
            };
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            await vkApi.CallAsync("messages.send", new VkParameters()
            {
                {"user_id", userid },
                {"random_id", Bot.rnd.Next() },
                {"attachment", gifs[Bot.rnd.Next(0, gifs.Length - 1)] },
            });
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg == null || string.IsNullOrEmpty(msg.Text);
        }
    }
}
