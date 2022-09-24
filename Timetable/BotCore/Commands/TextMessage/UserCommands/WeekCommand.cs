using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models.Bot;
using Timetable.Models;
using VkNet.Abstractions;
using VkNet.Model;
using Group = Timetable.Models.Group;
using QuestPDF.Fluent;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using System.Net;

namespace Timetable.BotCore.Commands.TextMessage
{
    public class WeekCommand : IVkBotCommand
    {
        private const long groupId = 208190108;

        public IVkApi vkApi { get; set; }

        private IEnumerable<TimeSpan> _intervals { get; set; }

        public WeekCommand(IVkApi api,IEnumerable<TimeSpan> intervals)
        {
            vkApi = api;
            _intervals = intervals;
        }

        public async Task Handle(object update, DatabaseContext db)
        {
            var msg = update as Message;
            long userid = msg.FromId.Value;
            var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            string message = user.Group != null ? "⌛ Ваше расписание генерируется.." : "❌ Для начала установите свою группу.Если у вас нету меню напишите «Начать» ❌";
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = message,
                UserId = userid,
                RandomId = ConcurrentRandom.Next(),
            });
            if (user.Group is null)
                return;
            var dateTime = msg.Text.ToLower().Contains("следующая") ? DtExtensions.LocalTimeNow().AddDays(7) : 
                                                                      DtExtensions.LocalTimeNow();
            var intervalLessons = GetWeekLessons(user.Group, dateTime, db);
            var doc = new TimetableDoc(intervalLessons, user.Group.GroupName);
            var img = doc.GenerateImages().FirstOrDefault();
            var photo = await UploadPhoto(img);
            await Task.Delay(1500);
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                UserId = userid,
                Attachments = new List<Photo>
                {
                    photo,
                },
                Message = $"📅 Ваше расписание с {intervalLessons.First().Days.First().Date} по {intervalLessons.First().Days.Last().Date} 📅",
                RandomId = ConcurrentRandom.Next(),
            });
        }


        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg != null && (msg.Text.ToLower().Contains("текущая неделя") ||
                                  msg.Text.ToLower().Contains("следующая неделя"));
        }

        /// <summary>
        /// Загружает фото на сервер для сохранения
        /// фото в личных сообщениях
        /// </summary>
        /// <param name="image"></param>
        /// <returns>
        /// Загруженное фото
        /// </returns>
        public async Task<Photo?> UploadPhoto(byte[] image)
        {
            var server = await vkApi.Photo.GetMessagesUploadServerAsync(groupId);
            using (HttpClient httpClient = new HttpClient())
            {
                using (var multipartFormContent = new MultipartFormDataContent())
                {
                    multipartFormContent.Add(new ByteArrayContent(image), "photo", "vkontakteebanytoeapi.png");

                    var response = await httpClient.PostAsync(server.UploadUrl, multipartFormContent);
                    response.EnsureSuccessStatusCode();
                    var resp = await response.Content.ReadAsStringAsync();
                    var photo = await vkApi.Photo.SaveMessagesPhotoAsync(resp);
                    return photo?.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Получить все дни недели вместе с парами.
        /// Такая архитектур потому что библиотека questPdf строит
        /// ячейки слева направо, а не снизу вверх.
        /// Т.е нам необходимо интервал и дни недели для построения
        /// расписания сверху вниз
        /// </summary>
        /// <param name="group"></param>
        /// <param name="dateTime"></param>
        /// <param name="db"></param>
        /// <returns>
        /// Пары содержащие дни недели вместе с лекциями
        /// </returns>
        private IEnumerable<Interval> GetWeekLessons(Group group, DateTime dateTime, DatabaseContext db)
        {
            var intervals = new List<Interval>();
            var weekDays = dateTime.GetWeekDays();
            foreach (var time in _intervals)
            {
                Interval interval = new Interval(time);
                foreach (var weekDay in weekDays)
                {
                    IList<Lesson> lessons = db.Lessons.Where(x => x.Group == group &&
                                                                  x.StartTime.Date == weekDay.Date &&
                                                                  x.StartTime.TimeOfDay == interval.Time).ToList();
                    Day day = new Day(weekDay, lessons);
                    interval.Days.Add(day);
                }
                intervals.Add(interval);
            }
            return intervals;
        }
    }
}
