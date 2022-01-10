using Microsoft.AspNetCore.Mvc;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using VkNet.Utils;

namespace Timetable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Экземпляр бота
        /// </summary>
        private readonly IVkBot _vkBot;


        public CallbackController(IConfiguration configuration, IVkBot bot)
        {
            _configuration = configuration;
            _vkBot = bot;
        }

        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {
            if (updates.Secret != _configuration["Secret"]) // Удостоверяемся, что уведомление пришло именно от сервера вк
            {
                return Unauthorized("Secret is invalid");
            }

            // Тип события
            switch (updates.Type)
            {
                // Ключ-подтверждение
                case "confirmation":
                    {
                        return Ok(_configuration["Confirmation"]);
                    }
                // Новый эвент (для работы с callback кнопками)
                case "message_event":
                    {
                        MessageEvent messageEvent = MessageEvent.FromJson(new VkResponse(updates.Object));
                        _vkBot.RegisterEvent(messageEvent);
                        break;
                    }
                // Новое сообщение
                case "message_new":
                    {
                        Message msg = Message.FromJson(new VkResponse(updates.Object));
                        _vkBot.RegisterMessage(msg);
                        break;
                    }
            }

            return Ok("ok");
        }
    }
}
