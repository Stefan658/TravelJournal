using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

using iTextSharp.text;
using iTextSharp.text.pdf;

using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;

namespace TravelJournal.Web.Controllers
{
    public class ExportController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserService _userService;

        // Temporar (până la auth)
        private const int DefaultUserId = 1;

        public ExportController(
            IEntryService entryService,
            ISubscriptionService subscriptionService,
            IUserService userService)
        {
            _entryService = entryService;
            _subscriptionService = subscriptionService;
            _userService = userService;
        }

        // GET: /Export/JournalPdf?journalId=1&userId=1
        [HttpGet]
        public ActionResult JournalPdf(int journalId, int? userId)
        {
            int uid = userId ?? DefaultUserId;

            // 1) user valid
            var user = _userService.GetById(uid);
            if (user == null)
                return HttpNotFound("User not found.");

            // 2) ✅ GATING CORECT: pe SubscriptionId (nu pe userId)
            if (!_subscriptionService.CanExportPdf(user.SubscriptionId))
            {
                return new HttpStatusCodeResult(
                    403,
                    "Upgrade required: PDF export is available only on Premium plan."
                );
            }

            // 3) date export
            var entries = _entryService.GetByJournal(journalId)?.ToList()
                          ?? new List<Entry>();

            // (optional) dacă vrei să excluzi soft-deleted, în caz că GetByJournal le include:
            // entries = entries.Where(e => !e.IsDeleted).ToList();

            // 4) generare PDF
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                doc.Add(new Paragraph($"TravelJournal - Export (JournalId={journalId})", titleFont));
                doc.Add(new Paragraph($"Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", normalFont));
                doc.Add(new Paragraph($"UserId: {uid} | SubscriptionId: {user.SubscriptionId}", normalFont));
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
                        table.AddCell(new PdfPCell(new Phrase($"{e.EntryId}", normalFont)));
                        table.AddCell(new PdfPCell(new Phrase($"{e.CreatedAt:yyyy-MM-dd}", normalFont)));

                        var text = $"{e.Title}\n{e.Content}";
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
