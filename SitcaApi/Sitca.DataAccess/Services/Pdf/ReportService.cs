using iText.Kernel.Pdf;
using iText.Layout;
using iText.Html2pdf;
using System.IO;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.Layout.Properties;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.Pdf;

public class ITextReportService : IReportService
{
  private readonly float MARGIN_TOP = 20f;
  private readonly float MARGIN_RIGHT = 20f;
  private readonly float MARGIN_BOTTOM = 50f;
  private readonly float MARGIN_LEFT = 20f;

  public byte[] GeneratePdfReport(string htmlContent, string orientation = "default")
  {

    using (var memoryStream = new MemoryStream())
    {
      var writer = new PdfWriter(memoryStream);
      var pdf = new PdfDocument(writer);

      // Configurar el tamaño y orientación de la página
      var pageSize = orientation.ToLower() == "landscape"
          ? PageSize.A4.Rotate()
          : PageSize.A4;

      pdf.SetDefaultPageSize(pageSize);

      // Crear el documento con márgenes
      var document = new Document(pdf);
      document.SetMargins(MARGIN_TOP, MARGIN_RIGHT, MARGIN_BOTTOM, MARGIN_LEFT);

      // Agregar pie de página con números de página
      int n = pdf.GetNumberOfPages();
      for (int i = 1; i <= n; i++)
      {
        document.ShowTextAligned(
            new Paragraph(string.Format("página {0} de {1}", i, n)),
            pageSize.GetWidth() / 2,
            15,
            i,
            TextAlignment.CENTER,
            VerticalAlignment.BOTTOM,
            0
        );
      }


      // Construir el HTML completo
      var html = $@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <style>
                            body {{ font-family: Arial, sans-serif; }}
                            .page-break {{ page-break-after: always; }}
                        </style>
                    </head>
                    <body>
                        {htmlContent}
                    </body>
                    </html>";

      // Convertir HTML a PDF
      ConverterProperties converterProperties = new ConverterProperties();
      HtmlConverter.ConvertToPdf(html, pdf, converterProperties);

      document.Close();
      return memoryStream.ToArray();
    }
  }

  public byte[] GenerateComplexPdfReport(PdfReportOptions options)
  {
    using (var memoryStream = new MemoryStream())
    {
      var writer = new PdfWriter(memoryStream);
      var pdf = new PdfDocument(writer);
      var pageSize = GetPageSize(options.Orientation);
      pdf.SetDefaultPageSize(pageSize);

      var document = new Document(pdf);
      document.SetMargins(
          options.Margins?.Top ?? MARGIN_TOP,
          options.Margins?.Right ?? MARGIN_RIGHT,
          options.Margins?.Bottom ?? MARGIN_BOTTOM,
          options.Margins?.Left ?? MARGIN_LEFT
      );

      if (options.AddPageNumbers)
      {
        AddPageNumbers(document, pageSize);
      }

      if (!string.IsNullOrEmpty(options.Header))
      {
        AddHeader(document, options.Header);
      }

      // Convertir HTML a PDF con opciones personalizadas
      var converterProperties = new ConverterProperties();

      HtmlConverter.ConvertToPdf(options.HtmlContent, pdf, converterProperties);

      if (!string.IsNullOrEmpty(options.Footer))
      {
        AddFooter(document, options.Footer);
      }

      document.Close();
      return memoryStream.ToArray();
    }
  }

  private PageSize GetPageSize(string orientation)
  {
    return orientation?.ToLower() == "landscape"
        ? PageSize.A4.Rotate()
        : PageSize.A4;
  }

  private void AddPageNumbers(Document document, PageSize pageSize)
  {
    int n = document.GetPdfDocument().GetNumberOfPages();
    for (int i = 1; i <= n; i++)
    {
      document.ShowTextAligned(
          new Paragraph($"página {i} de {n}"),
          pageSize.GetWidth() / 2,
          15,
          i,
          TextAlignment.CENTER,
          VerticalAlignment.BOTTOM,
          0
      );
    }
  }

  private void AddHeader(Document document, string headerText)
  {
    document.Add(new Paragraph(headerText)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFontSize(12));
  }

  private void AddFooter(Document document, string footerText)
  {
    document.Add(new Paragraph(footerText)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFontSize(10));
  }

}

