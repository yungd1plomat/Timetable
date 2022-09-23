using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using HtmlAgilityPack;
using Timetable.Models;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

using Group = Timetable.Models.Group; // Явно указываем т.к Group есть и в других namespace

namespace Timetable.BotCore.Workers
{
    public class Parser : IParser, IDisposable
    {
        public DatabaseContext db { get; set; }

        public ILogger _logger { get; set; }

        private bool disposed = false;

        private ILoggerFactory loggerFactory { get; set; }


        public Parser()
        {
            loggerFactory = LoggerFactory.Create(builder =>
            {
                //builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<Parser>();
            db = new DatabaseContext();
        }

        public void ClearCache()
        {
            var old = db.Lessons.Where(x => x.StartTime < DtExtensions.LocalTimeNow());
            db.Lessons.RemoveRange(old);
            db.SaveChanges();
        }

        public string CreateMD5(string input)
        {
            // Наиболее быстрый тип хеширования имеющий готовую реализацию
            using (MD5 md5 = MD5.Create()) 
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder(); // Быстрее чем +=
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private void ConfigureHeaders(ref HttpClient client, int page)
        {
            // Очищаем все заголовки (включая куки)
            client.DefaultRequestHeaders.Clear();

            // Добавляем юзерагент
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_7_1) AppleWebKit/5312 (KHTML, like Gecko) Chrome/38.0.853.0 Mobile Safari/5312");

            // Добавляем куки с номером страницы (страницы мотаются при помощи куков)
            client.DefaultRequestHeaders.Add("Cookie", $"rasp_default_aspx_ctl00_MainContent_ASPxPageControl1_grGroup=page{page}%7cconditions1%7c0%7c3%7chierarchy4%7c0%7c-1%7c1%7c-1%7c2%7c-1%7c3%7c-1%7cvisible4%7ct1%7ct2%7ct3%7ct4%7cwidth4%7ce%7ce%7ce%7c50px");
        }

        public void UpdateGroups()
        {
            _logger.LogInformation("Начат парсинг групп");
            // получаем существующие группы
            IEnumerable<long> allgroups = db.Groups.Select(x => x.GroupIdentifyId);

            HttpClient client = new HttpClient();
            ConfigureHeaders(ref client, 1);

            // Получаем html разметку страницы
            string html = client.GetStringAsync("http://asu.bspu.ru/Rasp/").GetAwaiter().GetResult();

            // Парсим кол-во страниц с группами
            int.TryParse(Regex.Match(html, @"Страница ([0-9]*) из ([0-9]*)").Groups[2].Value, out int pagecount); 
            if (pagecount == 0)
                return;

            // Проходимся по каждой странице
            for (int page = 1; page <= pagecount; page++)
            {
                // Настраиваем заголовки (чтобы была нужная страница с группой)
                ConfigureHeaders(ref client, page);

                html = client.GetStringAsync("http://asu.bspu.ru/Rasp/").GetAwaiter().GetResult();

                HtmlDocument htmlDoc = new HtmlDocument();

                // Загружаем html разметку
                htmlDoc.LoadHtml(html);

                // Выделяем таблицу с группами
                HtmlNodeCollection groups = htmlDoc.DocumentNode.SelectNodes("//tr[contains(@id,'ctl00_MainContent_ASPxPageControl1_grGroup_DXDataRow')]");

                // Проходимся по каждой строчке в таблице
                foreach (HtmlNode group in groups) 
                {
                    // Поле с информацией о группе содержит 7 аттрибутов, поэтому если встретится элемент
                    // с меньшим кол-вом аттрибутов (например поле поиска и т.д), то мы его пропускаем
                    if (group.ChildNodes.Count < 7) 
                        continue;

                    // Получаем Id группы на asu.bspu.ru
                    int.TryParse(Regex.Match(group.InnerHtml, @"group=(\d*)").Groups[1].Value, out int groupId);

                    // Получаем курс
                    int.TryParse(group.ChildNodes[4].InnerText, out int course); // Курс
                    
                    // Проверяем на ошибки парсинга и существование группы в бд
                    if (groupId == 0 || course == 0 || allgroups.Contains(groupId))
                        continue;

                    // Получаем имя группы
                    string groupName = group.ChildNodes[1].InnerText;

                    // Получаем факультет
                    string fakulty = group.ChildNodes[3].InnerText;

                    Group newGroup = new Group();
                    newGroup.GroupIdentifyId = groupId;
                    newGroup.GroupName = groupName;
                    newGroup.Course = course;
                    newGroup.Faculty = fakulty;

                    // Добавляем в бд
                    db.Groups.Add(newGroup);

                    _logger.LogInformation("Добавлена новая группа: {0}", groupName);
                }
            }
            // Сохраняем изменения
            db.SaveChanges();
            _logger.LogInformation("Парсинг групп завершен");

            // Освобождаем ресурсы (не люблю использовать using)
            client.Dispose();
        }

        private List<Lesson> GetLessons(HtmlNode table, Group group)
        {
            /* Структура таблицы
                 *                                17.12.2021 (дата)
                 * 08:00 | пр. Электронная информационно-образовательная среда | Лукманов А.Р.
                 * 09:30 |                                                     | ауд. 2-608 (комп.к)
                 * 09:50 | лек Науки о биологическом многообразии              | Сафиуллина Л.М.
                 * 11:20 |                                                     | ауд. 2-605
            */

            List<Lesson> lessons = new List<Lesson>();

            // Храним дату и время занятий
            DateTime? LastDate = null;

            // Проходимся по каждой записи таблицы
            foreach (HtmlNode row in table.ChildNodes) 
            {
                switch (row.GetAttributeValue("id", ""))
                {
                    // Перед нами дата (см. структуру таблицы)
                    case string id when id.Contains("ctl00_MainContent_ASPxGridView1_DXGroupRowExp"):
                        {
                            // Извлекаем дату
                            DateTime tmp = DateTime.ParseExact(row.ChildNodes[2].InnerText, "dd.MM.yyyy", null).Date;
                            if (tmp >= DtExtensions.LocalTimeNow().Date) // Не берем прошедшие занятия
                            {
                                LastDate = tmp;
                            }
                            else
                            {
                                LastDate = null;
                            }
                            break;
                        }
                    // Перед нами занятие (см. структуру таблицы)
                    case string id when id.Contains("ctl00_MainContent_ASPxGridView1_DXDataRow") && LastDate != null:
                        {
                            string subject;
                            string teacher;

                            // Парсинг для разных подгрупп
                            // Парсим время (у второй подгруппы нельзя получить время)
                            string tmpTime = row.ChildNodes[2].InnerHtml.Split("<br>")[0]; 
                            if (TimeSpan.TryParse(tmpTime, out TimeSpan result)) // Первая подгруппа
                            {
                                LastDate = LastDate.Value.ChangeTime(result);
                                subject = row.ChildNodes[3].InnerText.Replace("&quot;", "\"");
                                teacher = row.ChildNodes[4].InnerHtml.Replace("<br>", " ");
                            }
                            else // Вторая подгруппа
                            {
                                // Тут дата и время остается такой же как у первой подгруппы
                                subject = row.ChildNodes[2].InnerText.Replace("&quot;", "\"");
                                teacher = row.ChildNodes[3].InnerHtml.Replace("<br>", " ");
                            }
                            Lesson lesson = new Lesson();
                            lesson.StartTime = LastDate.Value;
                            lesson.Teacher = teacher;
                            lesson.Subject = subject;
                            lesson.Group = group;

                            lessons.Add(lesson);
                            break;
                        }
                }
            }
            return lessons;
        }

        public async Task UpdateTimetable()
        {
            _logger.LogInformation("Начато обновление расписания");
            // Получаем текущий семестр, на сайте
            // Осенний - 1
            // Весенний - 2
            // Поэтому считаем, что до НГ - 1 семестр, после - 2
            int semester = (DtExtensions.LocalTimeNow().Month > 8) ? 1 : 2;

            HttpClient client = new HttpClient();
            ConfigureHeaders(ref client, 0);

            var groups = db.Groups.ToList();
            int all = groups.Count();
            int parsed = 0;
            foreach (var group in groups)
            {
                // Получаем html разметку страницы
                string html = await client.GetStringAsync($"http://asu.bspu.ru/Rasp/Rasp.aspx?group={group.GroupIdentifyId}&sem={semester}");

                HtmlDocument htmlDoc = new HtmlDocument();

                // Загружаем html разметку
                htmlDoc.LoadHtml(html);

                // Выделяем таблицу с группами (она статична, нужна для хеширования)
                HtmlNode table = htmlDoc.DocumentNode.SelectSingleNode("//table[@id='ctl00_MainContent_ASPxGridView1_DXMainTable']");

                if (table != null)
                {
                    // Будем сверять хеш, чтобы понять, изменились ли данные на странице
                    string hash = CreateMD5(table.InnerHtml); 
                    if (group.Hash != hash) // Данные изменились (добавили новые занятия или убрали)
                    {
                        // Удаляем старые занятия
                        var oldLessons = db.Lessons.Where(x => x.Group == group).ToList();
                        db.Lessons.RemoveRange(oldLessons);

                        var lessons = GetLessons(table, group);
                        if (lessons.Any())
                        {
                            await db.Lessons.AddRangeAsync(lessons);

                            _logger.LogInformation($"Спарсили новое расписание у {group.GroupName}");
                        }
                        group.Hash = hash;
                    }
                }
                parsed++;
                _logger.LogInformation($"Осталось {all - parsed} групп");
            }
            await db.SaveChangesAsync();
            _logger.LogInformation("Парсинг расписания завершен");

            // Освобождаем ресурсы (не люблю использовать using)
            client.Dispose();
        }

        // Из рекомендаций Microsoft:
        // Отдавайте предпочтение комбинированному шаблону, реализующему как метод Dispose, так и деструктор
        public void Dispose() // Поможет быстрее удалить объект за счет GC.SuppressFinalize
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                    db.Dispose();
                    loggerFactory.Dispose();
                }
                // освобождаем неуправляемые объекты
                disposed = true;
            }
        }

        ~Parser()
        {
            Dispose(false);
        }
    }
}
