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
            await vkApi.Messages.SendAsync(new MessagesSendParams()
            {
                Message = "⌛ Ваше расписание генерируется..",
                UserId = userid,
                RandomId = ConcurrentRandom.Next(),
            });
            var user = db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            var dateTime = msg.Text.ToLower().Contains("следующая") ? DtExtensions.LocalTimeNow().AddDays(7) : 
                                                            DtExtensions.LocalTimeNow();
            var days = GetWeekLessons(user.Group, dateTime, db);
            var doc = new TimetableDoc(days, user.Group.GroupName);
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
                Message = $"📅 Ваше расписание с {days.First().Date} по {days.Last().Date} 📅",
                RandomId = ConcurrentRandom.Next(),
            });
        }


        public bool IsMatch(object update, DatabaseContext db)
        {
            var msg = update as Message;
            return msg != null && msg.Text.ToLower().Contains("текущая неделя") ||
                                  msg.Text.ToLower().Contains("следующая неделя");
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
        /// Получить все дни недели вместе с парами
        /// для каждого дня
        /// </summary>
        /// <param name="group"></param>
        /// <param name="dateTime"></param>
        /// <param name="db"></param>
        /// <returns>
        /// Дни недели с парами
        /// </returns>
        private IEnumerable<Day> GetWeekLessons(Group group, DateTime dateTime, DatabaseContext db)
        {
            var days = new List<Day>();
            var weekDays = dateTime.GetWeekDays();
            IEnumerable<Lesson> lessons = db.Lessons.Where(x => x.Group == group &&
                                                                x.StartTime.Date >= weekDays.First().Date &&
                                                                x.StartTime.Date <= weekDays.Last().Date).ToList();
            foreach (var weekDay in weekDays)
            {
                Day day = new Day(weekDay);
                foreach (var _interval in _intervals)
                {
                    var groupLessons = lessons.Where(x => x.StartTime.Date.Equals(weekDay.Date) &&
                                                          x.StartTime.TimeOfDay.Equals(_interval)).ToList();
                    Interval interval = new Interval(_interval, groupLessons);
                    day.Intervals.Add(interval);
                }
                days.Add(day);
            }
            return days;
        }
    }
}
