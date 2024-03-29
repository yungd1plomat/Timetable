﻿using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Timetable.BotCore.Commands.TextMessage
{
    /// <summary>
    /// Команда для добавления нового администратора
    /// </summary>
    public class AddAdminCommand : IVkBotCommand
    {
        public IVkApi vkApi { get; set; }

        public AddAdminCommand(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            try
            {
                var arguments = msg.Text.Split(' ');
                string screen_name = arguments[1].Split('/').Last(); // https://vk.com/musin007, получаем screen_name - musin007

                if (!long.TryParse(screen_name, out long userid))
                {
                    userid = vkApi.Users.Get(new string[] { screen_name })[0].Id; // Получаем id юзера (если задан адрес страницы)
                }

                var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();

                if (user != null)
                {
                    user.Admin = true;
                    user.Subscribtion = DateTime.MaxValue;
                    await vkApi.Messages.SendAsync(new MessagesSendParams()
                    {
                        Message = "🎊 Вас назначили администратором!\nНапишите «Начать», чтобы получить админское меню",
                        UserId = userid,
                        RandomId = ConcurrentRandom.Next(),
                    });
                }
                else
                {
                    await db.Users.AddAsync(new BotUser()
                    {
                        UserId = userid,
                        Admin = true,
                        Subscribtion = DateTime.MaxValue,
                    });
                }
                await db.SaveChangesAsync();
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    Message = $"☑ Пользователь {screen_name} успешно назначен администратором",
                    UserId = msg.FromId.Value,
                    RandomId = ConcurrentRandom.Next(),
                });
            }
            catch
            {
                await vkApi.Messages.SendAsync(new MessagesSendParams()
                {
                    Message = "Произошла ошибка при выполнении команды, проверьте синтаксис",
                    UserId = msg.FromId.Value,
                    RandomId = ConcurrentRandom.Next(),
                });
            }
        }

        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            if (msg != null)
            {
                var user = db.Users.Where(x => x.UserId == msg.FromId).FirstOrDefault();
                string text = msg.Text.ToLower();
                if (text.Contains("/addAdmin") && user.Admin == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
