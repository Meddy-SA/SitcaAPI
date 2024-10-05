using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.DataAccess.Services.Pdf
{
    public class ReportService : IReportService
    {
        private readonly IConverter _converter;
        public ReportService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GeneratePdfReport(string view, string orientation = "default")
        {
  

            var html = $@"
                       <!DOCTYPE html>
                       <html lang=""en"">
                       <head>
                           
                       </head>
                      <body>
                        {view}
                      </body>
                      </html>
                      ";

            GlobalSettings globalSettings = new GlobalSettings();
            globalSettings.ColorMode = ColorMode.Color;
            globalSettings.Orientation = orientation == "default" ? Orientation.Portrait: Orientation.Landscape;
            globalSettings.PaperSize = PaperKind.A4;
            globalSettings.Margins = new MarginSettings { Top = 5, Bottom = 25 };

            ObjectSettings objectSettings = new ObjectSettings();
            objectSettings.PagesCount = true;
            objectSettings.HtmlContent = html;
            WebSettings webSettings = new WebSettings();
            webSettings.DefaultEncoding = "utf-8";

            HeaderSettings headerSettings = new HeaderSettings();
            headerSettings.FontSize = 15;
            headerSettings.FontName = "Ariel";
            headerSettings.Right = "Page [page] of [toPage]";
            headerSettings.Line = true;
            //headerSettings.HtmUrl = "http://localhost:44367/Views/Shared/PdfHeader.html";

            FooterSettings footerSettings = new FooterSettings();
            footerSettings.FontSize = 12;
            footerSettings.FontName = "Ariel";
            footerSettings.Center = "[page] / [toPage]";
            footerSettings.Line = true;

            //objectSettings.HeaderSettings = headerSettings;
            objectSettings.FooterSettings = footerSettings;
            objectSettings.WebSettings = webSettings;
           

            HtmlToPdfDocument htmlToPdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };
            return _converter.Convert(htmlToPdfDocument);
        }
    }
}
