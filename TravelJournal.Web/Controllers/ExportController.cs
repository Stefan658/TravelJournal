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
        private readonly IPhotoService _photoService;


        

        public ExportController(
            IEntryService entryService,
            ISubscriptionService subscriptionService,
            IUserService userService,
            IPhotoService photoService)
        {
            _entryService = entryService;
            _subscriptionService = subscriptionService;
            _userService = userService;
            _photoService = photoService;
        }

        // GET: /Export/JournalPdf?journalId=1&userId=1
        [HttpGet]
        [Authorize]
        public ActionResult JournalPdf(int journalId, int? userId)
        {
            int uid =GetCurrentUserId();

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
        private int GetCurrentUserId()
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return 0;

            var user = _userService.GetByUsername(username);
            return user?.UserId ?? 0;
        }


        // GET: /Export/Export?entryId=13
        [HttpGet]
        [Authorize]
        public ActionResult Export(int entryId)
        {
            // 1) user autentificat
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0)
                return new HttpUnauthorizedResult();

            // 2) subscription + gating
            var sub = _userService.GetSubscription(currentUserId);
            var subId = sub?.SubscriptionId ?? 0;

            // PDF export: Premium only
            if (!_subscriptionService.CanExportPdf(subId))
                return new HttpStatusCodeResult(403, "Upgrade required: PDF export is available only on Premium plan.");

            // Photos in PDF: only if plan allows media upload (Explorer/Premium)
            var canIncludePhotos = _subscriptionService.CanUploadMedia(subId);

            // 3) ownership guard
            var entry = _entryService.GetByIdForUser(entryId, currentUserId);
            if (entry == null)
                return HttpNotFound("Entry not found or access denied.");

            // 4) pozele entry-ului (doar dacă avem voie să le includem)
            var photos = canIncludePhotos
                ? (_photoService.GetByEntry(entryId)?.ToList() ?? new List<Photo>())
                : new List<Photo>();

            // 5) generare PDF
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var hFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                doc.Add(new Paragraph("TravelJournal – Entry Export", titleFont));
                doc.Add(new Paragraph($"Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", normalFont));
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph(entry.Title ?? "", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));
                doc.Add(new Paragraph($"Location: {entry.Location ?? "-"}", normalFont));
                doc.Add(new Paragraph($"Created: {entry.CreatedAt:yyyy-MM-dd HH:mm}", normalFont));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(entry.Content ?? "", normalFont));

                // 6) include JPG/PNG în PDF doar dacă planul permite Media Upload
                if (canIncludePhotos && photos.Any())
                {
                    doc.Add(new Paragraph(" "));
                    doc.Add(new Paragraph("Photos", hFont));
                    doc.Add(new Paragraph(" "));

                    foreach (var p in photos)
                    {
                        // adaptează proprietatea în funcție de modelul tău (FilePath / Path / Url)
                        var rawPath = p.FilePath; // <-- dacă la tine se numește altfel, schimbă aici

                        if (string.IsNullOrWhiteSpace(rawPath))
                            continue;

                        string physicalPath = null;

                        // cazul 1: "~/Uploads/x.jpg"
                        if (rawPath.StartsWith("~"))
                        {
                            physicalPath = Server.MapPath(rawPath);
                        }
                        // cazul 2: "/Uploads/x.jpg" sau "Uploads/x.jpg" (relativ)
                        else if (!Path.IsPathRooted(rawPath) && !rawPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            var virtualPath = rawPath.StartsWith("/") ? ("~" + rawPath) : ("~/" + rawPath);
                            physicalPath = Server.MapPath(virtualPath);
                        }
                        // cazul 3: "C:\...\x.jpg" (deja fizic)
                        else if (Path.IsPathRooted(rawPath))
                        {
                            physicalPath = rawPath;
                        }
                        // cazul 4: URL http/https -> îl sărim (nu facem download acum)
                        else
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(physicalPath) || !System.IO.File.Exists(physicalPath))
                            continue;

                        try
                        {
                            var img = iTextSharp.text.Image.GetInstance(physicalPath);
                            img.ScaleToFit(500f, 350f); // safe pe A4
                            img.Alignment = Element.ALIGN_LEFT;
                            doc.Add(img);
                            doc.Add(new Paragraph(" "));
                        }
                        catch
                        {
                            // dacă un fișier e corupt / format neașteptat, nu vrem să crape exportul
                            continue;
                        }
                    }
                }

                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"entry_{entry.EntryId}.pdf");
            }
        }

    }
}
