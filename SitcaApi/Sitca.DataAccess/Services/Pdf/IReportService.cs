using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.Pdf
{
  public interface IReportService
  {
    public byte[] GeneratePdfReport(string htmlContent, string orientation = "default");
    byte[] GenerateComplexPdfReport(PdfReportOptions options);
  }
}
