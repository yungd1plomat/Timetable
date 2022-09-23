using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace Timetable.BotCore.Abstractions
{
    public interface IDocument
    {
        DocumentMetadata GetMetadata();
        void Compose(IDocumentContainer container);
    }
}
