using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TravelJournal.Services.Interfaces;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Web.Controllers
{
    public class ExportController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly ISubscriptionService _subscriptionService;

        // temporar (până la auth)
        private const int DefaultUserId = 1;

        public ExportController(IEntryService entryService, ISubscriptionService subscriptionService)
        {
            _entryService = entryService;
            _subscriptionService = subscriptionService;
        }

        // GET: /Export/JournalPdf?journalId=1
        public ActionResult JournalPdf(int journalId)
        {
            // ✅ GATING SERVER-SIDE (demonstrabil)
            if (!_subscriptionService.CanExportPdf(DefaultUserId))
            {
                return new HttpStatusCodeResult(403, "Upgrade required: PDF export is available only on Premium plan.");
            }

            var entries = _entryService.GetByJournal(journalId)?.ToList()
              ?? new List<Entry>();


            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                doc.Add(new Paragraph($"TravelJournal - Export (JournalId={journalId})", titleFont));
                doc.Add(new Paragraph($"Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", normalFont));
                doc.Add(new Paragraph(" ", normalFont));

                if (!entries.Any())
                {
                    doc.Add(new Paragraph("No entries found for this journal.", normalFont));
                }
                else
                {
                    var table = new PdfPTable(3) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 12f, 18f, 70f });

                    AddHeader(table, "Id");
                    AddHeader(table, "CreatedAt");
                    AddHeader(table, "Title/Content");

                    foreach (var e in entries)
                    {
                        // NOTE: adaptează numele câmpurilor după modelul tău Entry
                        table.AddCell(new PdfPCell(new Phrase($"{e.EntryId}", normalFont)));
                        table.AddCell(new PdfPCell(new Phrase($"{e.CreatedAt:yyyy-MM-dd}", normalFont)));

                        string text = "";
                        try
                        {
                            text = $"{e.Title}\n{e.Content}";
                        }
                        catch
                        {
                            // fallback dacă nu există Title/Content
                            text = e.ToString();
                        }

                        table.AddCell(new PdfPCell(new Phrase(text ?? "", normalFont)));
                    }

                    doc.Add(table);
                }

                doc.Close();

                var bytes = ms.ToArray();
                return File(bytes, "application/pdf", $"journal_{journalId}_export.pdf");
            }
        }

        private static void AddHeader(PdfPTable table, string text)
        {
            var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = BaseColor.LIGHT_GRAY
            };
            table.AddCell(cell);
        }
    }
}
