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
    /// Команда "Начать"
    /// </summary>
    public class StartCommand : IVkBotCommand
    {
        private MessageKeyboard mainkeyboard { get; set; } // Кнопки меню

        private MessageKeyboard Adminkeyboard { get; set; }

        public IVkApi vkApi {get;set;}

        public StartCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
            mainkeyboard = new KeyboardBuilder().AddButton("✍🏻 Сегодня", "today", KeyboardButtonColor.Positive)
                                .AddButton("🔍 Поиск", "find", KeyboardButtonColor.Default)
                                .AddLine()
                                .AddButton("👁 Завтра", "tomorrow", KeyboardButtonColor.Primary)
                                .AddButton("👀 Послезавтра", "after_tomorrow", KeyboardButtonColor.Primary)
                                .AddLine()
                                .AddButton("🕗 Текущая неделя", "this_week", KeyboardButtonColor.Primary)
                                .AddLine()
                                .AddButton("🕓 Следующая неделя", "next_week", KeyboardButtonColor.Primary)
                                .AddLine()
                                .AddButton("👥 Установить группу", "setgroup", KeyboardButtonColor.Positive)
                                //.AddLine()
                                //.AddButton("💰 Подписка", "subscribe", KeyboardButtonColor.Positive)
                                .SetInline(false)
                                .Build();
            Adminkeyboard = new KeyboardBuilder().AddButton("✍🏻 Сегодня", "today", KeyboardButtonColor.Positive)
                                .AddButton("🔍 Поиск", "find", KeyboardButtonColor.Default)
                                .AddLine()
                                .AddButton("👁 Завтра", "tomorrow", KeyboardButtonColor.Primary)
                                .AddButton("👀 Послезавтра", "after_tomorrow", KeyboardButtonColor.Primary)
                                .AddLine()
                                .AddButton("👥 Установить группу", "setgroup", KeyboardButtonColor.Positive)
                                .AddLine()
                                .AddButton("📈 Статистика", "statistics", KeyboardButtonColor.Positive)
                                .AddButton(@"¯\_(ツ)_/¯", "help", KeyboardButtonColor.Positive)
                                .SetInline(false)
                                .Build();
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            var keyboard = user.Admin.HasValue && user.Admin.Value ? Adminkeyboard : mainkeyboard;
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = "👥 Добро пожаловать в AdoBot v2.0 👥\n" +
                          "\n" +
                          "▶ Мои возможности:\n" +
                          "\n" +
                          "💥 Оповещение за 15 минут до пары" +
                          "\n" +
                          "🕧 Автообновление расписания каждый день" +
                          "\n" +
                          "⚡ Возможность посмотреть расписание на сегодня, завтра и послезавтра" +
                          "\n" +
                          "💎 Возможность поиска ближайшего предмета по преподавателю, предмету, времени или аудитории" +
                          "\n" +
                          "\n" +
                          "⌛ Пользование ботом бесплатно, поддержать https://vk.com/donut/adobot",
                RandomId = Bot.rnd.Next(),
                UserId = userid,
                Keyboard = keyboard,
            });

        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg != null && msg.Text.ToLower().Contains("начать");
        }
    }
}
