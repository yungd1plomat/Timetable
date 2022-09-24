using Timetable.BotCore.Abstractions;
using Timetable.BotCore.Workers;
using Timetable.Helpers;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using Timetable.BotCore.Commands.TextMessage;
using Timetable.BotCore.Commands.Callback;

namespace Timetable.BotCore
{
    public class Bot : IVkBot
    {
        /// <inheritdoc />
        public IVkApi _vkApi { get; set; }

        /// <inheritdoc />
        public IMonitor _monitor { get; set; }

        /// <summary>
        /// Команды бота
        /// </summary>
        public IEnumerable<IVkBotCommand> vkBotTextCommands { get; set; }
        private IVkBotCommand _registerUserCommand { get; set; }
        public IEnumerable<IVkBotCommand> vkBotCallbackCommands { get; set; }


        private readonly ILogger _logger;

        public Bot(string token)
        {
            _vkApi = new VkApi();
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<Bot>();

            Authorize(token);
            _monitor = new TimeMonitor(_vkApi, _logger);

            _registerUserCommand = new RegisterUserCommand(_vkApi);
            vkBotTextCommands = new IVkBotCommand[]
            {
                //new SubscribeCommand(_vkApi),
                new StartCommand(_vkApi),
                new SendCourseCommand(_vkApi),
                new GetTimetableCommand(_vkApi),
                new StatisticsCommand(_vkApi),
                new HelpCommand(_vkApi),
                new AddAdminCommand(_vkApi),
                new RemoveAdminCommand(_vkApi),
                new AddSubsCommand(_vkApi),
                new DeleteSubsCommand(_vkApi),
                new UserInfoCommand(_vkApi),
                new StickerCommand(_vkApi),
                new WeekCommand(_vkApi, _monitor.Intervals),
                new TimerCommand(_vkApi),
                new FindCommand(_vkApi),
            };
            vkBotCallbackCommands = new IVkBotCommand[]
            {
                new SendFakultyCommand(_vkApi),
                new SendGroupsCommand(_vkApi),
                new SetGroupCommand(_vkApi),
                /*new CreateBillCommand(_vkApi),
                new CheckBillCommand(_vkApi),
                new RejectBillCommand(_vkApi),*/
            };
            _monitor.StartMonitoring();
        }

        public void Authorize(string accessToken)
        {
            // Вк теперь генерирует токены случайной длины, проверять валидность токена
            // по длине было далбаебским решением
            // * можно дернуть какой нибудь метод вк чтобы чекнуть токен на валид
            _vkApi.Authorize(new ApiAuthParams()
            {
                AccessToken = accessToken
            });
            _logger.LogInformation("Успешно авторизовано");
        }

        public async void RegisterEvent(MessageEvent msgEvent)
        {
            try
            {
                using (DatabaseContext db = new DatabaseContext()) // DbContext не многопоточный, поэтому нужно создавать экземпляр кажлый раз
                {
                    foreach (IVkBotCommand command in vkBotCallbackCommands) // Обрабатываем эвенты (нажатия на callback кнопки)
                    {
                        if (command.IsMatch(msgEvent, db))
                        {
                            await command.Handle(msgEvent, db);
                            break;
                        }
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async void RegisterMessage(Message msg)
        {
            try
            {
                using (DatabaseContext db = new DatabaseContext()) // DbContext не многопоточный, поэтому нужно создавать экземпляр кажлый раз
                {
                    if (_registerUserCommand.IsMatch(msg, db)) // Регистрируем юзера
                    {
                        await _registerUserCommand.Handle(msg, db);
                    }

                    foreach (IVkBotCommand vkBotEvent in vkBotTextCommands) // Обрабатываем текстовые команды
                    {
                        if (vkBotEvent.IsMatch(msg, db))
                        {
                            await vkBotEvent.Handle(msg, db);
                            break;
                        }
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            GC.Collect();
        }
    }
}
