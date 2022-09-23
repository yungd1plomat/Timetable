using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Timetable.Helpers;
using Timetable.Models.Bot;

namespace Timetable.BotCore
{
    public class TimetableDoc : IDocument
    {
        private static IList<byte[]> _images = new List<byte[]>()
        {
            Properties.Resources._2,
            Properties.Resources._3,
        };

        private const string fontFamily = "Montserrat";

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        private IEnumerable<Day> weekLessons { get; set; }

        private string groupName { get; set; }


        public TimetableDoc(IEnumerable<Day> weekLessons, string groupName)
        {
            //QuestPDF.Drawing.FontManager.RegisterFont("")
            this.weekLessons = weekLessons;
            this.groupName = groupName;
        }

        private byte[] GetImg()
        {
            int id = ConcurrentRandom.Next(0, _images.Count() - 1);
            return _images[id];
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(60.1f, 30, Unit.Centimetre);
                
                page.Background().Image(GetImg(), ImageScaling.Resize);
                //page.PageColor("#000115");
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Height(1f, Unit.Centimetre)
                             .AlignCenter()
                             .AlignMiddle()
                             .Text("Powered by AdoBot (https://vk.com/adobot). Created by d1plomat")
                             .FontFamily(fontFamily)
                             .FontSize(15)
                             .FontColor("#ff007a")
                             .Bold();
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Element(HeaderStyle)
                     .Text(text =>
                     {
                         text.AlignCenter();
                         text.Span($"Расписание с ").FontColor("#fff");
                         text.Span(weekLessons.First().Date).FontColor("#ff007a");
                         text.Span(" по ").FontColor("#fff");
                         text.Line(weekLessons.Last().Date).FontColor("#ff007a");
                         text.Span($"{groupName}").Bold()
                                                  .FontSize(20)
                                                  .FontColor("#fff");
                     });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(5);

                column.Item().Element(ComposeTable);
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(5f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                    columns.ConstantColumn(7.86f, Unit.Centimetre);
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("День / Время").SemiBold();
                    foreach (var interval in weekLessons.First().Intervals)
                    {
                        header.Cell().Element(HeaderCellStyle).Text($"{interval.Time.ToString(@"hh\:mm")} - " +
                                                                    $"{interval.Time.Add(TimeSpan.FromHours(1.5)).ToString(@"hh\:mm")}")
                                                              .SemiBold();
                    }
                });

                foreach (var day in weekLessons)
                {
                    table.Cell().Element(CellStyle).Column(column =>
                    {
                        column.Spacing(7);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(RowStyle).Text(day.DayName).SemiBold().FontSize(14);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(RowStyle).Text(day.Date).SemiBold().FontSize(14);
                        });
                    });
                    foreach (var interval in day.Intervals)
                    {
                        string color = interval.Lessons.Any() ? "#181a1b" : "#546575";
                        table.Cell().Element(CellStyle)
                                    .Column(column =>
                                    {
                                        column.Spacing(7);
                                        foreach (var lesson in interval.Lessons)
                                        {
                                            column.Item().Row(row =>
                                            {
                                                row.RelativeItem().Element(RowStyle).Text(lesson.Subject).SemiBold();
                                            });
                                            // Рисуем разделяющую черту, если у нас несколько пар в одно время
                                            // Например у разных подгрупп.
                                            // Нам нужно нарисовать черточку снизу только
                                            // если есть еще пары, кроме текущей
                                            var item = lesson != interval.Lessons.Last() ? column.Item().BorderBottom(1) : column.Item();
                                            item.Row(row =>
                                            {
                                                row.RelativeItem().Element(RowStyle).Text(lesson.Teacher).Italic(true);
                                            });
                                        }
                                    });
                    }
                }
            });
        }


        private static IContainer HeaderStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.FontSize(18)
                                                    .Bold()
                                                    .FontFamily(fontFamily)).Height(2.8f, Unit.Centimetre)
                                                                            .AlignCenter()
                                                                            .AlignMiddle();
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.FontFamily(fontFamily).FontSize(13)
                                                                           .FontColor("#fff")
                                                                           .Bold()).BorderColor("#e8e6e3")
                                                                                   .Border(1)
                                                                                   .AlignCenter()
                                                                                   .AlignMiddle();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.FontFamily(fontFamily)
                                                    .FontSize(13)
                                                    .FontColor("#fff")).MinHeight(4, Unit.Centimetre)
                                                                       .BorderColor("#e8e6e3")
                                                                       .Border(1)
                                                                       .AlignCenter()
                                                                       .AlignMiddle();
        }

        private static IContainer RowStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.FontFamily(fontFamily)
                                                    .FontSize(13)
                                                    .FontColor("#fff")).AlignCenter()
                                                                       .AlignMiddle()
                                                                       .PaddingRight(1.5f, Unit.Millimetre)
                                                                       .PaddingLeft(1.5f, Unit.Millimetre);
        }
    }
}
