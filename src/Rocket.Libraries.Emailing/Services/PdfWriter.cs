using DinkToPdf;
using System;
using System.IO;

namespace Rocket.Libraries.Emailing.Services
{
    class PdfWriter
    {
        public byte[] GetPdfBytes(string htmlContent)
        {
            LoadLib();
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },

                    }
                }
            };

            var converter = new BasicConverter(new PdfTools());
            return converter.Convert(doc);
        }


        private void LoadLib()
        {
            var architectureFolder = (IntPtr.Size == 8) ? "64bit" : "32bit";
            var wkHtmlToPdfPath = Path.Combine(AppContext.BaseDirectory, $"libs/{architectureFolder}/");
            foreach (var file in Directory.GetFiles(wkHtmlToPdfPath))
            {
                var targetFile = $"{AppContext.BaseDirectory}/{Path.GetFileName(file)}";
                if (!File.Exists(targetFile))
                {
                    File.Copy(file, targetFile);
                }
            }
            /*var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(wkHtmlToPdfPath);*/
        }
    }
}
