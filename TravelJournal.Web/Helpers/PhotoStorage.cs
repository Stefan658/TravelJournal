using System;
using System.IO;
using System.Web;
using System.Linq;

namespace TravelJournal.Web.Helpers
{
    public static class PhotoStorage
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
        private const int MaxBytes = 5 * 1024 * 1024; // 5MB

        public static string Save(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
                throw new ArgumentException("No file provided.");

            if (file.ContentLength > MaxBytes)
                throw new ArgumentException("File too large. Max allowed size is 5MB.");

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
                throw new ArgumentException("Invalid file type. Allowed: .jpg, .jpeg, .png, .webp");

            // MIME check (nu e perfect infailibil, dar e standard si suficient pt barem)
            var mime = (file.ContentType ?? "").ToLowerInvariant();
            if (!AllowedMimeTypes.Contains(mime))
                throw new ArgumentException("Invalid image content type.");

            var fileName = $"{Guid.NewGuid():N}{ext}";

            var relativeDir = "~/Content/uploads/photos";
            var relativePath = $"{relativeDir}/{fileName}";
            var absoluteDir = HttpContext.Current.Server.MapPath(relativeDir);
            var absolutePath = HttpContext.Current.Server.MapPath(relativePath);

            if (!Directory.Exists(absoluteDir))
                Directory.CreateDirectory(absoluteDir);

            file.SaveAs(absolutePath);

            return relativePath;
        }
    }
}
