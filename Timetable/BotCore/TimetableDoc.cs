using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Concurrent;
using Timetable.Helpers;
using Timetable.Models.Bot;

namespace Timetable.BotCore
{
    public class TimetableDoc : IDocument
    {
        private static ConcurrentQueue<byte[]> _images;

        private const string fontFamily = "Montserrat";

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        private IEnumerable<Interval> intervals { get; set; }

        private string groupName { get; set; }

        static TimetableDoc()
        {
            FontManager.RegisterFont(File.OpenRead("./Assets/Fonts/Montserrat-Bold.ttf"));
            _images = new ConcurrentQueue<byte[]>();
            _images.Enqueue(Properties.Resources._1);
            _images.Enqueue(Properties.Resources._2);
            _images.Enqueue(Properties.Resources._3);
            _images.Enqueue(Properties.Resources._4);
            _images.Enqueue(Properties.Resources._7);
            _images.Enqueue(Properties.Resources._8);
            _images.Enqueue(Properties.Resources._9);
        }


        public TimetableDoc(IEnumerable<Interval> intervals, string groupName)
        {
            this.intervals = intervals;
            this.groupName = groupName;
        }

        private byte[]? GetImg()
        {
            if (_images.TryDequeue(out byte[]? img))
            {
                _images.Enqueue(img);
            }
            return img;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(60.1f, 35, Unit.Centimetre);
                var image = GetImg() ?? Properties.Resources._3;
                page.Background().Image(image, ImageScaling.Resize);
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
            var firstDay = intervals.First().Days.First();
            var lastDay = intervals.First().Days.Last();
            container.Element(HeaderStyle)
                     .Text(text =>
                     {
                         text.AlignCenter();
                         text.Span($"Расписание с ").FontColor("#fff");
                         text.Span(firstDay.Date).FontColor("#ff007a");
                         text.Span(" по ").FontColor("#fff");
                         text.Line(lastDay.Date).FontColor("#ff007a");
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
                    columns.ConstantColumn(9.17f, Unit.Centimetre);
                    columns.ConstantColumn(9.17f, Unit.Centimetre);
                    columns.ConstantColumn(9.17f, Unit.Centimetre);
                    columns.ConstantColumn(9.17f, Unit.Centimetre);
                    columns.ConstantColumn(9.17f, Unit.Centimetre);
                    columns.ConstantColumn(9.17f, Unit.Centimetre);
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Время / День").SemiBold();
                    foreach (var day in intervals.First().Days)
                    {
                        // Если в данный день нету пар, то подсвечиваем его
                        // красным цветом
                        var color = intervals.Select(x => x.Days)
                                             .Where(x => x.Any(y => y.Date == day.Date &&
                                                                    y.Lessons.Any()))
                                             .Any() ?
                                             "#fff" : "ff0073";
                        header.Cell().Element(HeaderCellStyle).Text(text =>
                        {
                            text.AlignCenter();
                            text.Line(day.DayName).FontColor(color);
                            text.Span(day.Date).FontColor(color);
                        });
                    }
                });
                int num = 1;
                foreach (var interval in intervals)
                {
                    table.Cell().Element(CellStyle).Column(column =>
                    {
                        column.Spacing(7);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(RowStyle).BorderColor("#e8e6e3")
                                                                .BorderBottom(0.5f)
                                                                .PaddingBottom(7)
                                                                .Text(interval.Time.ToString(@"hh\:mm")).FontSize(16).SemiBold();
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(RowStyle).Text(interval.Time.Add(TimeSpan.FromHours(1.5)).ToString(@"hh\:mm")).FontSize(16).SemiBold();
                        });
                    });
                    foreach (var day in interval.Days)
                    {
                        table.Cell().Element(CellStyle)
                                    .Column(column =>
                                    {
                                        column.Spacing(7);
                                        foreach (var lesson in day.Lessons)
                                        {
                                            column.Item().Row(row =>
                                            {
                                                row.RelativeItem().Element(RowStyle).Text(lesson.Subject).SemiBold();
                                            });
                                            // Рисуем разделяющую черту, если у нас несколько пар в одно время
                                            // Например у разных подгрупп.
                                            // Нам нужно нарисовать черточку снизу только
                                            // если есть еще пары, кроме текущей
                                            var item = lesson != day.Lessons.Last() ? column.Item().BorderColor("#e8e6e3")
                                                                                                   .BorderBottom(1)
                                                                                                   .PaddingBottom(5) : column.Item();
                                            item.Row(row =>
                                            {
                                                row.RelativeItem().Element(RowStyle).Text(lesson.Teacher).Italic(true);
                                            });
                                        }
                                    });

                    }
                    num++;
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
            return container.DefaultTextStyle(x => x.FontFamily(fontFamily)
                                                    .FontSize(14)
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
